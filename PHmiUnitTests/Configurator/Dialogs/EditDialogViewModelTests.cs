﻿using System;
using System.ComponentModel;
using Moq;
using NUnit.Framework;
using PHmiConfigurator.Dialogs;

namespace PHmiUnitTests.Configurator.Dialogs {
    [TestFixture]
    public class EditDialogViewModelTests {
        [SetUp]
        public void SetUp() {
            _viewModel = new EditDialogViewModel();
            _view = new Mock<IEditDialog<DataErrorInfo>>();
            _viewModel.View = _view.Object;
        }

        public class DataErrorInfo : IDataErrorInfo, INotifyPropertyChanged {
            public bool HasError;

            public string this[string columnName] {
                get { throw new NotImplementedException(); }
            }

            public string Error {
                get { return HasError ? "Error" : string.Empty; }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private class EditDialogViewModel : EditDialogViewModel<DataErrorInfo> { }

        private Mock<IEditDialog<DataErrorInfo>> _view;
        private EditDialogViewModel _viewModel;

        private static readonly object[] DataErrorInfoTestCases = {new DataErrorInfo(), null};

        private static readonly object[] DataErrorInfoTestCasesValidInvalid = {
            new DataErrorInfo {HasError = false},
            new DataErrorInfo {HasError = true},
            null
        };

        [Test]
        public void CancelCommandExecutedTest() {
            _viewModel.CancelCommand.Execute(null);
            _view.VerifySet(v => v.DialogResult = false, Times.Once());
        }

        [Test]
        public void EntityErrorPropertyChangedRaisesOkCommandCanExecutedChanged() {
            var entity = new DataErrorInfo();
            _viewModel.Entity = entity;
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            entity.OnPropertyChanged("Error");
            Assert.IsTrue(raised);
        }

        [Test]
        public void EntityPropertyChangedDoesNotRaiseOkCommandCanExecutedChangedIfCancelCommandExecuted() {
            var entity = new DataErrorInfo();
            _viewModel.Entity = entity;
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.CancelCommand.Execute(null);
            entity.OnPropertyChanged(null);
            Assert.IsFalse(raised);
        }

        [Test]
        public void EntityPropertyChangedDoesNotRaiseOkCommandCanExecutedChangedIfEntitySetOther() {
            var entity = new DataErrorInfo();
            _viewModel.Entity = entity;
            _viewModel.Entity = null;
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            entity.OnPropertyChanged(null);
            Assert.IsFalse(raised);
        }

        [Test]
        public void EntityPropertyChangedDoesNotRaiseOkCommandCanExecutedChangedIfOkCommandExecuted() {
            var entity = new DataErrorInfo();
            _viewModel.Entity = entity;
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.OkCommand.Execute(null);
            entity.OnPropertyChanged(null);
            Assert.IsFalse(raised);
        }

        [Test]
        [TestCaseSource("DataErrorInfoTestCasesValidInvalid")]
        public void OkCommandCanExecuteReturnsEntityHasError(DataErrorInfo entity) {
            _viewModel.Entity = entity;
            if (entity == null)
                Assert.IsFalse(_viewModel.OkCommand.CanExecute(null));
            else
                Assert.AreEqual(string.IsNullOrEmpty(entity.Error), _viewModel.OkCommand.CanExecute(null));
        }

        [Test]
        public void OkCommandExecutedTest() {
            _viewModel.OkCommand.Execute(null);
            _view.VerifySet(v => v.DialogResult = true, Times.Once());
        }

        [Test]
        [TestCaseSource("DataErrorInfoTestCases")]
        public void SetEntityRaisesOkCommandCanExecuteChanged(DataErrorInfo entity) {
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.Entity = entity;
            Assert.IsTrue(raised);
        }

        [Test]
        [TestCaseSource("DataErrorInfoTestCases")]
        public void SetEntityRaisesPropertyChanged(DataErrorInfo entity) {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Entity")
                    raised = true;
            };
            _viewModel.Entity = entity;
            Assert.IsTrue(raised);
        }
    }
}