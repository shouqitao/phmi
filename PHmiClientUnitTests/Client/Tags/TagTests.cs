using System;
using NUnit.Framework;
using PHmiClient.Tags;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Tags {
    public class WhenUsingTag : Specification {
        protected Func<string> DescriptionGetter;
        protected int Id;
        protected string Name;
        internal Tag<int?> Tag;

        protected override void EstablishContext() {
            base.EstablishContext();
            Id = RandomGenerator.GetRandomInt32();
            Name = "Name";
            DescriptionGetter = () => "Description";
            Tag = new TagStub(Id, Name, DescriptionGetter);
        }

        private class TagStub : Tag<int?> {
            public TagStub(int id, string name, Func<string> descriptionGetter) : base(
                new DispatcherService(), id, name, descriptionGetter) { }
        }

        public class ThenIdReturns : WhenUsingTag {
            [Test]
            public void Test() {
                Assert.That(Tag.Id, Is.EqualTo(Id));
            }
        }

        public class ThenNameReturns : WhenUsingTag {
            [Test]
            public void Test() {
                Assert.That(Tag.Name, Is.EqualTo(Name));
            }
        }

        public class ThenDescriptionReturns : WhenUsingTag {
            [Test]
            public void Test() {
                Assert.That(Tag.Description, Is.EqualTo(DescriptionGetter.Invoke()));
            }
        }

        public class AndValuePropertyChangedRaises : WhenUsingTag {
            protected int RaisedCount;

            protected override void EstablishContext() {
                base.EstablishContext();
                RaisedCount = 0;
                Tag.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "Value")
                        RaisedCount++;
                };
            }

            private void Verify() {
                Assert.That(RaisedCount, Is.EqualTo(1));
            }

            public class IfValueIsSet : AndValuePropertyChangedRaises {
                [Test]
                public void Test() {
                    Tag.Value = RandomGenerator.GetRandomInt32();
                    Verify();
                }
            }

            public class IfUpdateValueIsSet : AndValuePropertyChangedRaises {
                [Test]
                public void Test() {
                    Tag.UpdateValue(RandomGenerator.GetRandomInt32());
                    Verify();
                }
            }
        }

        public class ThenValueIsNull : WhenUsingTag {
            [Test]
            public void Test() {
                Assert.That(Tag.Value, Is.Null);
            }
        }

        public class AndUpdateValueInvoked : WhenUsingTag {
            protected int ValueForUpdate;

            protected override void EstablishContext() {
                base.EstablishContext();
                ValueForUpdate = RandomGenerator.GetRandomInt32();
                Tag.UpdateValue(ValueForUpdate);
            }

            public class ThenValueIsUpdated : AndUpdateValueInvoked {
                [Test]
                public void Test() {
                    Assert.That(Tag.Value, Is.EqualTo(ValueForUpdate));
                }
            }
        }

        public class AndValueIsSet : WhenUsingTag {
            protected int SettedValue;

            protected override void EstablishContext() {
                base.EstablishContext();
                SettedValue = RandomGenerator.GetRandomInt32();
                Tag.Value = SettedValue;
            }

            public class ThenValueIsSet : AndValueIsSet {
                [Test]
                public void Test() {
                    Assert.That(Tag.Value, Is.EqualTo(SettedValue));
                }
            }

            public class AndUpdateValueInvoked1 : AndValueIsSet {
                protected int ValueForUpdate;

                protected override void EstablishContext() {
                    base.EstablishContext();
                    ValueForUpdate = SettedValue > 0 ? SettedValue - 1 : 1;
                    Tag.UpdateValue(ValueForUpdate);
                }

                public class ThenValueIsNotUpdated : AndUpdateValueInvoked1 {
                    [Test]
                    public void Test() {
                        Assert.That(Tag.Value, Is.Not.EqualTo(ValueForUpdate));
                    }
                }
            }

            public class AndIsWrittenIsGotten : AndValueIsSet {
                protected bool IsWritten;

                protected override void EstablishContext() {
                    base.EstablishContext();
                    IsWritten = Tag.IsWritten;
                }

                public class ThenIsWrittenIsTrue : AndIsWrittenIsGotten {
                    [Test]
                    public void Test() {
                        Assert.That(IsWritten, Is.True);
                    }
                }
            }

            public class AndGetWrittenValueInvoked : AndValueIsSet {
                protected int? WrittenValue;

                protected override void EstablishContext() {
                    base.EstablishContext();
                    WrittenValue = Tag.GetWrittenValue();
                }

                public class ThenIsWrittenIsFalse : AndGetWrittenValueInvoked {
                    [Test]
                    public void Test() {
                        Assert.That(Tag.IsWritten, Is.False);
                    }
                }

                public class ThenValueReturned : AndGetWrittenValueInvoked {
                    [Test]
                    public void Test() {
                        Assert.That(WrittenValue, Is.EqualTo(Tag.Value));
                    }
                }

                public class AndUpdateValueInvoked2 : AndGetWrittenValueInvoked {
                    protected int? ValueForUpdate;

                    protected override void EstablishContext() {
                        base.EstablishContext();
                        ValueForUpdate = SettedValue > 0 ? SettedValue - 1 : 1;
                        Tag.UpdateValue(ValueForUpdate);
                    }

                    public class ThenValueUpdated : AndUpdateValueInvoked2 {
                        [Test]
                        public void Test() {
                            Assert.That(Tag.Value, Is.EqualTo(ValueForUpdate));
                        }
                    }
                }
            }
        }

        public class AndValueIsGotten : WhenUsingTag {
            protected int? GottenValue;

            protected override void EstablishContext() {
                base.EstablishContext();
                GottenValue = Tag.Value;
            }

            public class AndIsReadGotten : AndValueIsGotten {
                protected bool IsRead;

                protected override void EstablishContext() {
                    base.EstablishContext();
                    IsRead = Tag.IsRead;
                }

                public class ThenIsReadIsTrue : AndIsReadGotten {
                    [Test]
                    public void Test() {
                        Assert.That(IsRead, Is.True);
                    }
                }

                public class AndIsReadGottenAgain : AndIsReadGotten {
                    protected override void EstablishContext() {
                        base.EstablishContext();
                        IsRead = Tag.IsRead;
                    }

                    public class ThenIsReadIsFalse : AndIsReadGottenAgain {
                        [Test]
                        public void Test() {
                            Assert.That(IsRead, Is.False);
                        }
                    }
                }
            }
        }
    }
}