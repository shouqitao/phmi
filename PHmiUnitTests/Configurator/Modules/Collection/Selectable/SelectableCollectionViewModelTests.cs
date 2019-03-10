﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Moq;
using NUnit.Framework;
using PHmiConfigurator.Dialogs;
using PHmiConfigurator.Modules.Collection.Selectable;
using PHmiModel.Interfaces;

namespace PHmiUnitTests.Configurator.Modules.Collection.Selectable {
    [TestFixture]
    public class SelectableCollectionViewModelTests {
        [SetUp]
        public void SetUp() {
            _service = new Service();
            _contextStub = new Mock<IModelContext>();
            _service.ContextFactoryStub.Setup(f => f.Create(It.IsAny<string>(), true))
                .Returns(_contextStub.Object);
            _editDialogStub = new Mock<IEditDialog<Meta>>();
            _viewModel = new ViewModel(_service) {EditDialog = _editDialogStub.Object};
        }

        public class Service : CollectionViewModelTests.Service, ISelectableCollectionService { }

        public class Meta : IDataErrorInfo {
            public string this[string columnName] {
                get { throw new NotImplementedException(); }
            }

            public string Error { get; private set; }
        }

        public class DataErrorInfo : IDataErrorInfo, INotifyPropertyChanged, INamedEntity {
            public string this[string columnName] {
                get { throw new NotImplementedException(); }
            }

            public string Error { get; private set; }

            public string Name { get; set; }
            public int Id { get; set; }

#pragma warning disable 0067

            public event PropertyChangedEventHandler PropertyChanged;

#pragma warning restore 0067
        }

        public class Selector : INamedEntity, IRepository {
            private readonly List<DataErrorInfo> _repository = new List<DataErrorInfo>();

            public bool ThrowOnGetRepository;
            public string Name { get; set; }
            public int Id { get; set; }

            public ICollection<T> GetRepository<T>() {
                if (ThrowOnGetRepository)
                    throw new Exception();
                return (ICollection<T>) _repository;
            }
        }

        private class ViewModel : SelectableCollectionViewModel<DataErrorInfo, Meta, Selector> {
            private bool _isValid;

            public IEditDialog<Meta> EditDialog;

            public ViewModel(Service service) : base(service) { }

            public override string Name {
                get { return "Name"; }
            }

            public override bool IsValid {
                get { return _isValid; }
            }

            protected override IEditDialog<Meta> CreateAddDialog() {
                return EditDialog;
            }

            protected override IEditDialog<Meta> CreateEditDialog() {
                return EditDialog;
            }

            protected override string[] GetCopyData(DataErrorInfo item) {
                throw new NotImplementedException();
            }

            protected override string[] GetCopyHeaders() {
                throw new NotImplementedException();
            }

            protected override void SetCopyData(DataErrorInfo item, string[] data) {
                throw new NotImplementedException();
            }

            public void SetIsValid(bool isValid) {
                _isValid = isValid;
            }
        }

        private Service _service;
        private Mock<IModelContext> _contextStub;
        private Mock<IEditDialog<Meta>> _editDialogStub;
        private ViewModel _viewModel;

        private static readonly Selector[] SelectorTestCases = {new Selector(), null};

        private static readonly object[] BooleanTestCases = {true, false};

        [Test]
        [TestCaseSource("SelectorTestCases")]
        public void AddCommandCanExecuteChangedRaisesWhenSelectorIsSet(Selector selector) {
            var raised = false;
            _viewModel.AddCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.CurrentSelector = selector;
            Assert.IsTrue(raised);
        }

        [Test]
        [TestCaseSource("SelectorTestCases")]
        public void AddCommandCanExecuteReturnsCurrentSelectorNotNull(Selector selector) {
            _viewModel.CurrentSelector = selector;
            Assert.AreEqual(selector != null, _viewModel.AddCommand.CanExecute(null));
        }

        [Test]
        [TestCaseSource("BooleanTestCases")]
        public void AddCommandTest(bool result) {
            _editDialogStub.Setup(d => d.ShowDialog()).Returns(result);
            _viewModel.Reload();
            var selector = new Selector();
            _viewModel.CurrentSelector = selector;
            _viewModel.AddCommand.Execute(null);
            _editDialogStub.Verify();
            _contextStub.Verify(c => c.AddTo(It.IsAny<DataErrorInfo>()),
                result ? Times.Once() : Times.Never());
            if (result)
                Assert.AreEqual(selector.GetRepository<DataErrorInfo>().Single(),
                    _viewModel.Collection.Single());
        }

        [Test]
        public void LoadCollectionClearsColellectionIfCurrentSelectorIsNull() {
            _viewModel.Reload();
            var selector = new Selector();
            var info1 = new DataErrorInfo {Id = 1, Name = "2"};
            var info2 = new DataErrorInfo {Id = 2, Name = "1"};
            selector.GetRepository<DataErrorInfo>().Add(info1);
            selector.GetRepository<DataErrorInfo>().Add(info2);
            _viewModel.CurrentSelector = selector;
            _viewModel.CurrentSelector = null;

            Assert.IsEmpty(_viewModel.Collection);
        }

        [Test]
        public void LoadCollectionGetsCurrentSelectorsDigitalTagsToColellectionOrederedById() {
            _viewModel.Reload();
            var i = new Selector();
            var info1 = new DataErrorInfo {Id = 2, Name = "1"};
            var info2 = new DataErrorInfo {Id = 1, Name = "2"};
            i.GetRepository<DataErrorInfo>().Add(info1);
            i.GetRepository<DataErrorInfo>().Add(info2);
            _viewModel.CurrentSelector = i;

            Assert.AreEqual(2, _viewModel.Collection.Count);
            Assert.AreEqual(info2, _viewModel.Collection.First());
            Assert.AreEqual(info1, _viewModel.Collection.Last());
        }

        [Test]
        public void LoadCollectionRaisesCloseEventIfExceptionOccured() {
            _viewModel.Reload();
            var raised = false;
            _viewModel.Closed += (sender, args) => { raised = true; };
            var selector = new Selector {ThrowOnGetRepository = true};
            _viewModel.CurrentSelector = selector;
            Assert.IsTrue(raised);
        }

        [Test]
        public void LoadCollectionShowsExceptionDialogIfExceptionOccured() {
            _viewModel.Reload();
            var selector = new Selector {ThrowOnGetRepository = true};
            _viewModel.CurrentSelector = selector;
            _service.DialogHelperStub.Verify(h => h.Exception(It.IsAny<Exception>(), It.IsAny<object>()));
        }

        [Test]
        public void ReloadClearsSelectors() {
            var one = new Selector {Name = "one", Id = 1};
            var two = new Selector {Name = "two", Id = 2};
            var selectors = new[] {one, two}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(new Selector[0].AsQueryable());
            _viewModel.Reload();
            Assert.IsEmpty(_viewModel.Selectors);
        }

        [Test]
        public void ReloadDoesNotPreserveCurrentSelectorIfItCannotFindIt() {
            var one = new Selector {Name = "one", Id = 1};
            var two = new Selector {Name = "two", Id = 2};
            var selectors = new[] {one, two}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            _viewModel.CurrentSelector = two;

            var one2 = new Selector {Name = "one", Id = 3};
            var two2 = new Selector {Name = "two", Id = 4};
            var selectors2 = new[] {one2, two2}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors2);
            _viewModel.Reload();
            Assert.IsNull(_viewModel.CurrentSelector);
        }

        [Test]
        public void ReloadLoadsIoDevicesOrederedByName() {
            var one = new Selector {Name = "io2", Id = 1};
            var two = new Selector {Name = "io1", Id = 2};
            var selectors = new[] {one, two}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            Assert.AreEqual(2, _viewModel.Selectors.Count);
            Assert.AreEqual(two, _viewModel.Selectors.First());
            Assert.AreEqual(one, _viewModel.Selectors.Last());
        }

        [Test]
        public void ReloaDoesNotSetCurrentSelectorToSingleIfCurrentSelectorWasNotNull() {
            var one = new Selector {Name = "one", Id = 1};
            var two = new Selector {Name = "two", Id = 2};
            var selectors = new[] {one, two}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            _viewModel.CurrentSelector = two;

            var single = new Selector {Name = "one", Id = 1};
            var selectors2 = new[] {single}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors2);
            _viewModel.Reload();
            Assert.IsNull(_viewModel.CurrentSelector);
        }

        [Test]
        public void ReloadSetsCurrentSelectorToSingle() {
            var single = new Selector {Name = "one", Id = 1};
            var selectors = new[] {single}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            Assert.AreEqual(single, _viewModel.CurrentSelector);
        }

        [Test]
        public void ReloadTriesToPreserveCurrentSelector() {
            var one = new Selector {Name = "one", Id = 1};
            var two = new Selector {Name = "two", Id = 2};
            var selectors = new[] {one, two}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors);
            _viewModel.Reload();
            _viewModel.CurrentSelector = two;

            var one2 = new Selector {Name = "one", Id = 1};
            var two2 = new Selector {Name = "two", Id = 2};
            var selectors2 = new[] {one2, two2}.AsQueryable();
            _contextStub.Setup(c => c.Get<Selector>()).Returns(selectors2);
            _viewModel.Reload();

            Assert.AreEqual(two2, _viewModel.CurrentSelector);
        }

        [Test]
        [TestCaseSource("SelectorTestCases")]
        public void SetCurrentSelectorDoesNotAllowIfContextHasChanges(Selector selector) {
            _viewModel.Reload();
            var oldSelector = new Selector();
            _viewModel.CurrentSelector = oldSelector;
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _viewModel.CurrentSelector = selector;
            _service.DialogHelperStub.Verify(
                h => h.Message(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK,
                    MessageBoxImage.Information, It.IsAny<object>()),
                Times.Once());
            Assert.AreEqual(oldSelector, _viewModel.CurrentSelector);
        }

        [Test]
        [TestCaseSource("SelectorTestCases")]
        public void SetCurrentSelectorRaisesPropertyChanged(Selector selector) {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "CurrentSelector")
                    raised = true;
            };
            _viewModel.CurrentSelector = selector;
            Assert.IsTrue(raised);
        }
    }
}