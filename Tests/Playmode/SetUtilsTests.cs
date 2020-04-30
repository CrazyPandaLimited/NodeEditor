using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class SetUtilsTests
    {
        [Test]
        public void SetOnce_Should_Succeed()
        {
            var tester = new Tester();

            Assert.That( () => tester.ValueOnce = "value", Throws.Nothing );
            Assert.That( tester.ValueOnce, Is.EqualTo( "value" ) );
        }

        [Test]
        public void SetOnce_Should_Throw_OnSecondSet()
        {
            var tester = new Tester();
            tester.ValueOnce = "value";

            Assert.That( () => tester.ValueOnce = "value2", Throws.InvalidOperationException );
            Assert.That( tester.ValueOnce, Is.EqualTo( "value" ) );
        }

        [Test]
        public void SetOnceOrNull_Should_Succeed()
        {
            var tester = new Tester();

            Assert.That( () => tester.ValueOnceOrNull = "value", Throws.Nothing );
            Assert.That( tester.ValueOnceOrNull, Is.EqualTo( "value" ) );

            Assert.That( () => tester.ValueOnceOrNull = null, Throws.Nothing );
            Assert.That( tester.ValueOnceOrNull, Is.Null );

            Assert.That( () => tester.ValueOnceOrNull = "value2", Throws.Nothing );
            Assert.That( tester.ValueOnceOrNull, Is.EqualTo( "value2" ) );
        }

        [Test]
        public void SetOnceOrNull_Should_Throw_OnSecondSet()
        {
            var tester = new Tester();
            tester.ValueOnceOrNull = "value";

            Assert.That( () => tester.ValueOnceOrNull = "value2", Throws.InvalidOperationException );
            Assert.That( tester.ValueOnceOrNull, Is.EqualTo( "value" ) );
        }

        private class Tester
        {
            private string _valueOnce;
            private string _valueOnceOrNull;

            public string ValueOnce
            {
                get => _valueOnce;
                set => this.SetOnce( ref _valueOnce, value );
            }

            public string ValueOnceOrNull
            {
                get => _valueOnceOrNull;
                set => this.SetOnceOrNull( ref _valueOnceOrNull, value );
            }
        }
    }
}