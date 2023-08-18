// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSliderPath : OsuTestScene
    {
        private readonly SmoothPath drawablePath;
        private SliderPath path;

        public TestSceneSliderPath()
        {
            Child = drawablePath = new SmoothPath
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            path = new SliderPath();
        });

        protected override void Update()
        {
            base.Update();

            if (path != null)
            {
                List<Vector2> vertices = new List<Vector2>();
                path.GetPathToProgress(vertices, 0, 1);

                drawablePath.Vertices = vertices;
            }
        }

        [Test]
        public void TestEmptyPath()
        {
        }

        [TestCase(PathType.Linear)]
        [TestCase(PathType.Bezier)]
        [TestCase(PathType.Catmull)]
        [TestCase(PathType.PerfectCurve)]
        public void TestSingleSegment(PathType type)
            => AddStep("create path", () => path.ControlPoints.AddRange(createSegment(type, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));

        [TestCase(PathType.Linear)]
        [TestCase(PathType.Bezier)]
        [TestCase(PathType.Catmull)]
        [TestCase(PathType.PerfectCurve)]
        public void TestMultipleSegment(PathType type)
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero));
                path.ControlPoints.AddRange(createSegment(type, new Vector2(0, 100), new Vector2(100), Vector2.Zero));
            });
        }

        [Test]
        public void TestAddControlPoint()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100))));
            AddStep("add point", () => path.ControlPoints.Add(new PathControlPoint { Position = new Vector2(100) }));
        }

        [Test]
        public void TestInsertControlPoint()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(100))));
            AddStep("insert point", () => path.ControlPoints.Insert(1, new PathControlPoint { Position = new Vector2(0, 100) }));
        }

        [Test]
        public void TestRemoveControlPoint()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("remove second point", () => path.ControlPoints.RemoveAt(1));
        }

        [Test]
        public void TestChangePathType()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("change type to bezier", () => path.ControlPoints[0].Type = PathType.Bezier);
        }

        [Test]
        public void TestAddSegmentByChangingType()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100), new Vector2(100, 0))));
            AddStep("change second point type to bezier", () => path.ControlPoints[1].Type = PathType.Bezier);
        }

        [Test]
        public void TestRemoveSegmentByChangingType()
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100), new Vector2(100, 0)));
                path.ControlPoints[1].Type = PathType.Bezier;
            });

            AddStep("change second point type to null", () => path.ControlPoints[1].Type = null);
        }

        [Test]
        public void TestRemoveSegmentByRemovingControlPoint()
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100), new Vector2(100, 0)));
                path.ControlPoints[1].Type = PathType.Bezier;
            });

            AddStep("remove second point", () => path.ControlPoints.RemoveAt(1));
        }

        [TestCase(2)]
        [TestCase(4)]
        public void TestPerfectCurveFallbackScenarios(int points)
        {
            AddStep("create path", () =>
            {
                switch (points)
                {
                    case 2:
                        path.ControlPoints.AddRange(createSegment(PathType.PerfectCurve, Vector2.Zero, new Vector2(0, 100)));
                        break;

                    case 4:
                        path.ControlPoints.AddRange(createSegment(PathType.PerfectCurve, Vector2.Zero, new Vector2(0, 100), new Vector2(100), new Vector2(100, 0)));
                        break;
                }
            });
        }

        [Test]
        public void TestLengthenLastSegment()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("lengthen last segment", () => path.ExpectedDistance.Value = 300);
        }

        [Test]
        public void TestShortenLastSegment()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("shorten last segment", () => path.ExpectedDistance.Value = 150);
        }

        [Test]
        public void TestShortenFirstSegment()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("shorten first segment", () => path.ExpectedDistance.Value = 50);
        }

        [Test]
        public void TestShortenToZeroLength()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("shorten to 0 length", () => path.ExpectedDistance.Value = 0);
        }

        [Test]
        public void TestShortenToNegativeLength()
        {
            AddStep("create path", () => path.ControlPoints.AddRange(createSegment(PathType.Linear, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));
            AddStep("shorten to -10 length", () => path.ExpectedDistance.Value = -10);
        }

        [Test]
        public void TestGetSegmentEnds()
        {
            var positions = new[]
            {
                Vector2.Zero,
                new Vector2(100, 0),
                new Vector2(100),
                new Vector2(200, 100),
            };
            double[] distances = { 100d, 200d, 300d };

            AddStep("create path", () => path.ControlPoints.AddRange(positions.Select(p => new PathControlPoint(p, PathType.Linear))));
            AddAssert("segment ends are correct", () => path.GetSegmentEnds().SequenceEqual(distances.Select(d => d / 300)));
            AddAssert("segment end positions recovered", () => path.GetSegmentEnds().Select(p => path.PositionAt(p)).SequenceEqual(positions.Skip(1)));
            AddStep("lengthen last segment", () => path.ExpectedDistance.Value = 400);
            AddAssert("segment ends are correct", () => path.GetSegmentEnds().SequenceEqual(distances.Select(d => d / 400)));
            AddAssert("segment end positions recovered", () => path.GetSegmentEnds().Select(p => path.PositionAt(p)).SequenceEqual(positions.Skip(1)));
            AddStep("shorten last segment", () => path.ExpectedDistance.Value = 150);
            AddAssert("segment ends are correct", () => path.GetSegmentEnds().SequenceEqual(distances.Select(d => d / 150)));
            AddAssert("segment end positions not recovered", () => path.GetSegmentEnds().Select(p => path.PositionAt(p)).SequenceEqual(new[]
            {
                positions[1],
                new Vector2(100, 50),
                new Vector2(100, 50),
            }));
        }

        private List<PathControlPoint> createSegment(PathType type, params Vector2[] controlPoints)
        {
            var points = controlPoints.Select(p => new PathControlPoint { Position = p }).ToList();
            points[0].Type = type;
            return points;
        }
    }
}
