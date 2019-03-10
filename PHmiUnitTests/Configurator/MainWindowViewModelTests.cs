using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils.ViewInterfaces;
using PHmiConfigurator;
using PHmiConfigurator.Modules;
using PHmiConfigurator.Modules.Collection;
using PHmiResources.Loc;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Configurator {
    [TestFixture]
    public class MainWindowViewModelTests {
        [SetUp]
        public void SetUp() {
            _service = new Service();
            _windowStub = new Mock<IWindow>();
            _viewModel = new MainWindowViewModel(_service) {View = _windowStub.Object};
        }

        public class Service : IMainWindowService {
            public Mock<IDialogHelper> DialogHelperStub = new Mock<IDialogHelper>();

            public IDialogHelper DialogHelper {
                get { return DialogHelperStub.Object; }
            }
        }

        private class MuduleStubWithoutParameterlessConstructor {
            public MuduleStubWithoutParameterlessConstructor(object parameter) { }
        }

        private class ModuleStub { }

        private class InputElement : InputElementBase { }

        private class OtherInputElement : InputElementBase { }

#pragma warning disable 0067

        private abstract class InputElementBase : IInputElement {
            public bool Focused;

            public void RaiseEvent(RoutedEventArgs e) {
                throw new NotImplementedException();
            }

            public void AddHandler(RoutedEvent routedEvent, Delegate handler) {
                throw new NotImplementedException();
            }

            public void RemoveHandler(RoutedEvent routedEvent, Delegate handler) {
                throw new NotImplementedException();
            }

            public bool CaptureMouse() {
                throw new NotImplementedException();
            }

            public void ReleaseMouseCapture() {
                throw new NotImplementedException();
            }

            public bool CaptureStylus() {
                throw new NotImplementedException();
            }

            public void ReleaseStylusCapture() {
                throw new NotImplementedException();
            }

            public bool Focus() {
                Focused = true;
                return true;
            }

            public bool IsMouseOver { get; private set; }
            public bool IsMouseDirectlyOver { get; private set; }
            public bool IsMouseCaptured { get; private set; }
            public bool IsStylusOver { get; private set; }
            public bool IsStylusDirectlyOver { get; private set; }
            public bool IsStylusCaptured { get; private set; }
            public bool IsKeyboardFocusWithin { get; private set; }
            public bool IsKeyboardFocused { get; private set; }
            public bool IsEnabled { get; private set; }
            public bool Focusable { get; set; }

            public event MouseButtonEventHandler PreviewMouseLeftButtonDown;

            public event MouseButtonEventHandler MouseLeftButtonDown;

            public event MouseButtonEventHandler PreviewMouseLeftButtonUp;

            public event MouseButtonEventHandler MouseLeftButtonUp;

            public event MouseButtonEventHandler PreviewMouseRightButtonDown;

            public event MouseButtonEventHandler MouseRightButtonDown;

            public event MouseButtonEventHandler PreviewMouseRightButtonUp;

            public event MouseButtonEventHandler MouseRightButtonUp;

            public event MouseEventHandler PreviewMouseMove;

            public event MouseEventHandler MouseMove;

            public event MouseWheelEventHandler PreviewMouseWheel;

            public event MouseWheelEventHandler MouseWheel;

            public event MouseEventHandler MouseEnter;

            public event MouseEventHandler MouseLeave;

            public event MouseEventHandler GotMouseCapture;

            public event MouseEventHandler LostMouseCapture;

            public event StylusDownEventHandler PreviewStylusDown;

            public event StylusDownEventHandler StylusDown;

            public event StylusEventHandler PreviewStylusUp;

            public event StylusEventHandler StylusUp;

            public event StylusEventHandler PreviewStylusMove;

            public event StylusEventHandler StylusMove;

            public event StylusEventHandler PreviewStylusInAirMove;

            public event StylusEventHandler StylusInAirMove;

            public event StylusEventHandler StylusEnter;

            public event StylusEventHandler StylusLeave;

            public event StylusEventHandler PreviewStylusInRange;

            public event StylusEventHandler StylusInRange;

            public event StylusEventHandler PreviewStylusOutOfRange;

            public event StylusEventHandler StylusOutOfRange;

            public event StylusSystemGestureEventHandler PreviewStylusSystemGesture;

            public event StylusSystemGestureEventHandler StylusSystemGesture;

            public event StylusButtonEventHandler StylusButtonDown;

            public event StylusButtonEventHandler PreviewStylusButtonDown;

            public event StylusButtonEventHandler PreviewStylusButtonUp;

            public event StylusButtonEventHandler StylusButtonUp;

            public event StylusEventHandler GotStylusCapture;

            public event StylusEventHandler LostStylusCapture;

            public event KeyEventHandler PreviewKeyDown;

            public event KeyEventHandler KeyDown;

            public event KeyEventHandler PreviewKeyUp;

            public event KeyEventHandler KeyUp;

            public event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;

            public event KeyboardFocusChangedEventHandler GotKeyboardFocus;

            public event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;

            public event KeyboardFocusChangedEventHandler LostKeyboardFocus;

            public event TextCompositionEventHandler PreviewTextInput;

            public event TextCompositionEventHandler TextInput;
        }

#pragma warning restore 0067

        private class ModuleBase : IModule {
            public bool Saved;

            public string ConnectionString { get; set; }

            public event EventHandler Closed;

            public bool HasChanges { get; set; }
            public bool IsValid { get; set; }

            public void Save() {
                Saved = true;
            }

            public ImageSource ImageSource { get; set; }

            public void RaiseClosed() {
                Closed(this, EventArgs.Empty);
            }
        }

        private class Module : ModuleBase { }

        private class AnotherModule : ModuleBase { }

        private Service _service;
        private Mock<IWindow> _windowStub;
        private MainWindowViewModel _viewModel;

        private static readonly object[] OpenModuleCommandCanExecuteTestSource = {
            new object[] {new NpgConnectionParameters(), true},
            new object[] {null, false}
        };

        private static readonly INpgConnectionParameters[] ConnectionParametersesTestCases
            = {null, new NpgConnectionParameters()};

        private static readonly object[] ConnectionParametersTestCases = {
            new object[] {null, null},
            new object[] {new NpgConnectionParameters(), null},
            new object[] {null, true},
            new object[] {new NpgConnectionParameters(), true},
            new object[] {null, false},
            new object[] {new NpgConnectionParameters(), false}
        };

        private static readonly string[] DatabasesCases = {"DataBase", "OtherDatabase", "YetAnotherDatabase"};

        [Test]
        public void AddModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.AddModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.AddCommand.CanExecute(null)).Returns(false);
            _viewModel.AddModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.AddCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.AddCommand.CanExecute(null)).Returns(true);
            _viewModel.AddModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.AddCommand.Execute(null), Times.Once());
        }

        [Test]
        public void CloseCommandCanExecuteReturnsIfConnectionParametersAreNotNull() {
            Assert.IsNull(_viewModel.ConnectionParameters);
            Assert.IsFalse(_viewModel.CloseCommand.CanExecute(null));
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsNotNull(_viewModel.ConnectionParameters);
            Assert.IsTrue(_viewModel.CloseCommand.CanExecute(null));
        }

        [Test]
        public void CloseCommandSetsConnectionParametersToNull() {
            var parameters = new NpgConnectionParameters();
            _viewModel.ConnectionParameters = parameters;
            Assert.AreEqual(parameters, _viewModel.ConnectionParameters);
            _viewModel.CloseCommand.Execute(null);
            Assert.IsNull(_viewModel.ConnectionParameters);
        }

        [Test]
        public void CloseModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.CloseModuleCommand.Execute(null));
            var viewModelStub = new Mock<IModuleViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.CloseCommand.CanExecute(null)).Returns(false);
            _viewModel.CloseModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.CloseCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.CloseCommand.CanExecute(null)).Returns(true);
            _viewModel.CloseModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.CloseCommand.Execute(null), Times.Once());
        }

        [Test]
        [TestCaseSource("ConnectionParametersesTestCases")]
        public void ConnectionParametersSetAsksToSaveIfModulesHaveChanges(
            INpgConnectionParameters connectionParameters) {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModule<Module>();
            var module = (Module) _viewModel.Modules.Last();
            module.HasChanges = true;
            _viewModel.OpenModule<AnotherModule>();
            _viewModel.ConnectionParameters = connectionParameters;
            _service.DialogHelperStub.Verify(h => h.Message(
                It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question, It.IsAny<object>()));
        }

        [Test]
        [TestCaseSource("ConnectionParametersesTestCases")]
        public void ConnectionParametersSetClosesAllModules(INpgConnectionParameters connectionParameters) {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModule<Module>();
            _viewModel.OpenModule<AnotherModule>();
            _viewModel.ConnectionParameters = connectionParameters;
            Assert.IsEmpty(_viewModel.Modules);
        }

        [Test]
        [TestCaseSource("ConnectionParametersesTestCases")]
        public void ConnectionParametersSetDoesNothingIfModulesHaveChangesAndAreNotValidAndUserSaidYes(
            INpgConnectionParameters connectionParameters) {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModule<Module>();
            var module = (Module) _viewModel.Modules.Last();
            module.HasChanges = true;
            module.IsValid = false;
            _viewModel.OpenModule<AnotherModule>();
            var anotherModule = (AnotherModule) _viewModel.Modules.Last();
            _service.DialogHelperStub
                .Setup(h => h.Message(
                    It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, It.IsAny<object>()))
                .Returns(true);
            _viewModel.ConnectionParameters = connectionParameters;
            _service.DialogHelperStub.Verify();
            Assert.AreEqual(2, _viewModel.Modules.Count);
            Assert.AreSame(_viewModel.Modules.First(), module);
            Assert.IsFalse(module.Saved);
            Assert.AreSame(_viewModel.Modules.Last(), anotherModule);
            Assert.IsFalse(anotherModule.Saved);
            Assert.AreSame(conParamsStub.Object, _viewModel.ConnectionParameters);
            _service.DialogHelperStub.Verify(h => h.Message(
                Res.ValidationErrorMessage, Res.ValidationError, MessageBoxButton.OK, MessageBoxImage.Error,
                It.IsAny<object>()));
        }

        [Test]
        [TestCaseSource("ConnectionParametersTestCases")]
        public void ConnectionParametersSetIfModulesHaveChanges(INpgConnectionParameters connectionParameters,
            bool? result) {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModule<Module>();
            var module = (Module) _viewModel.Modules.Last();
            module.HasChanges = true;
            module.IsValid = true;
            _viewModel.OpenModule<AnotherModule>();
            var anotherModule = (AnotherModule) _viewModel.Modules.Last();
            _service.DialogHelperStub
                .Setup(h => h.Message(
                    It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question, It.IsAny<object>()))
                .Returns(result);
            _viewModel.ConnectionParameters = connectionParameters;
            _service.DialogHelperStub.Verify();
            if (result == null) {
                Assert.AreEqual(2, _viewModel.Modules.Count);
                Assert.AreSame(_viewModel.Modules.First(), module);
                Assert.IsFalse(module.Saved);
                Assert.AreSame(_viewModel.Modules.Last(), anotherModule);
                Assert.IsFalse(anotherModule.Saved);
                Assert.AreSame(conParamsStub.Object, _viewModel.ConnectionParameters);
            }

            if (result == true) {
                Assert.IsEmpty(_viewModel.Modules);
                Assert.IsTrue(module.Saved);
                Assert.IsFalse(anotherModule.Saved);
                Assert.AreEqual(connectionParameters, _viewModel.ConnectionParameters);
            }

            if (result == false) {
                Assert.IsEmpty(_viewModel.Modules);
                Assert.IsFalse(module.Saved);
                Assert.IsFalse(anotherModule.Saved);
                Assert.AreEqual(connectionParameters, _viewModel.ConnectionParameters);
            }
        }

        [Test]
        public void ConnectionParametersSetRaisesCloseCommandCanExecuteChanged() {
            var raised = false;
            _viewModel.CloseCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsTrue(raised);
        }

        [Test]
        public void ConnectionParametersSetRaisesExportCommandCanExecuteChanged() {
            var raised = false;
            _viewModel.ExportCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsTrue(raised);
        }

        [Test]
        public void ConnectionParametersSetRaisesOpenModuleCommandCanExecuteChanged() {
            var raised = false;
            _viewModel.OpenModuleCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsTrue(raised);
        }

        [Test]
        public void ConnectionParametersSetRaisesTitleChanged() {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Title")
                    raised = true;
            };
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsTrue(raised);
        }

        [Test]
        public void CopyModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.CopyModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.CopyCommand.CanExecute(null)).Returns(false);
            _viewModel.CopyModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.CopyCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.CopyCommand.CanExecute(null)).Returns(true);
            _viewModel.CopyModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.CopyCommand.Execute(null), Times.Once());
        }

        [Test]
        public void DeleteModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.DeleteModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.DeleteCommand.CanExecute(null)).Returns(false);
            _viewModel.DeleteModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.DeleteCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.DeleteCommand.CanExecute(null)).Returns(true);
            _viewModel.DeleteModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.DeleteCommand.Execute(null), Times.Once());
        }

        [Test]
        public void EditModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.EditModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.EditCommand.CanExecute(null)).Returns(false);
            _viewModel.EditModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.EditCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.EditCommand.CanExecute(null)).Returns(true);
            _viewModel.EditModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.EditCommand.Execute(null), Times.Once());
        }

        [Test]
        public void ExitCommandTest() {
            _viewModel.View = _windowStub.Object;
            _viewModel.ExitCommand.Execute(null);
            _windowStub.Verify(w => w.Close());
        }

        [Test]
        public void ExportCommandCanExecuteReturnsIfConnectionParametersAreNotNull() {
            Assert.IsNull(_viewModel.ConnectionParameters);
            Assert.IsFalse(_viewModel.ExportCommand.CanExecute(null));
            _viewModel.ConnectionParameters = new NpgConnectionParameters();
            Assert.IsNotNull(_viewModel.ConnectionParameters);
            Assert.IsTrue(_viewModel.ExportCommand.CanExecute(null));
        }

        [Test]
        public void HasChangesReturnsSingleModuleHasChanges() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            Assert.IsFalse(_viewModel.HasChanges);
            module.HasChanges = true;
            Assert.IsTrue(_viewModel.HasChanges);
        }

        [Test]
        public void HasChangesReturnsSingleModuleHasChangesWhenMoreItemsAreInModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(object));
            Assert.IsFalse(_viewModel.HasChanges);
            module.HasChanges = true;
            Assert.IsTrue(_viewModel.HasChanges);
        }

        [Test]
        public void HasChangesReturnsTrueIfOneModuleHasChangesAndMoreItemsAndModulesAreInModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(object));
            _viewModel.OpenModuleCommand.Execute(typeof(AnotherModule));
            Assert.IsFalse(_viewModel.HasChanges);
            module.HasChanges = true;
            Assert.IsTrue(_viewModel.HasChanges);
        }

        [Test]
        public void HasChangesReturnsTrueIfOneModuleHasChangesAndMoreModulesAreInModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(AnotherModule));
            Assert.IsFalse(_viewModel.HasChanges);
            module.HasChanges = true;
            Assert.IsTrue(_viewModel.HasChanges);
        }

        [Test]
        public void IsValidReturnsSingleModuleIsValid() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            Assert.IsFalse(_viewModel.IsValid);
            module.IsValid = true;
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsSingleModuleIsValidWhenMoreItemsAreInModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(object));
            Assert.IsFalse(_viewModel.IsValid);
            module.IsValid = true;
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsTrueIfAllModulesAreValid() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(AnotherModule));
            var anotherModule = (AnotherModule) _viewModel.Modules.Single(m => m is AnotherModule);
            Assert.IsFalse(_viewModel.IsValid);
            module.IsValid = true;
            Assert.IsFalse(_viewModel.IsValid);
            anotherModule.IsValid = true;
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsTrueIfAllModulesAreValidAndMoreItemsAreInModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            _viewModel.OpenModuleCommand.Execute(typeof(object));
            _viewModel.OpenModuleCommand.Execute(typeof(AnotherModule));
            var anotherModule = (AnotherModule) _viewModel.Modules.Single(m => m is AnotherModule);
            Assert.IsFalse(_viewModel.IsValid);
            module.IsValid = true;
            Assert.IsFalse(_viewModel.IsValid);
            anotherModule.IsValid = true;
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void ModuleClosedEventRemovesItFromModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single();
            module.RaiseClosed();
            Assert.IsEmpty(_viewModel.Modules);
        }

        [Test]
        public void ModulesIsEmptyAtStart() {
            Assert.IsEmpty(_viewModel.Modules);
        }

        [Test]
        [TestCaseSource("OpenModuleCommandCanExecuteTestSource")]
        public void OpenModuleCommandCanExecuteTest(INpgConnectionParameters connectionParameters,
            bool expected) {
            _viewModel.ConnectionParameters = connectionParameters;
            Assert.AreEqual(expected, _viewModel.OpenModuleCommand.CanExecute(null));
        }

        [Test]
        public void OpenModuleCommandThrowsIfParameterIsNotType() {
            TestDelegate del = () => _viewModel.OpenModuleCommand.Execute(new object());
            Assert.Catch<ArgumentException>(del);
        }

        [Test]
        public void OpenModuleCommandThrowsIfParameterIsNull() {
            TestDelegate del = () => _viewModel.OpenModuleCommand.Execute(null);
            Assert.Catch<ArgumentNullException>(del);
        }

        [Test]
        public void OpenModuleCreatesNewModuleAndAddsItToModules() {
            _viewModel.OpenModule<ModuleStub>();
            Assert.IsTrue(_viewModel.Modules.Single() is ModuleStub);
        }

        [Test]
        public void OpenModuleDoesNotCreateModuleIfOneIsPresentOfThatType() {
            _viewModel.OpenModule<ModuleStub>();
            object present = _viewModel.Modules.Single();
            _viewModel.OpenModule<ModuleStub>();
            Assert.AreSame(present, _viewModel.Modules.Single());
        }

        [Test]
        public void OpenModulePassesConectionStringToModule() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            conParamsStub.SetupGet(p => p.ConnectionString).Returns("ConStr");
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModule<Module>();
            Assert.AreEqual(typeof(Module), _viewModel.Modules.Single().GetType());
            Assert.AreEqual("ConStr", ((Module) _viewModel.Modules.Single()).ConnectionString);
        }

        [Test]
        public void OpenModuleThrowsIfParameterTypeHasNotParameterlessConstructor() {
            TestDelegate del = _viewModel.OpenModule<MuduleStubWithoutParameterlessConstructor>;
            Assert.Catch<ArgumentException>(del);
        }

        [Test]
        public void OpenModuleTriesToFocusModuleIfItIsIInputElement() {
            _viewModel.OpenModule<InputElement>();
            Assert.IsTrue(((InputElement) _viewModel.Modules.Single()).Focused);
        }

        [Test]
        public void OpenModuleTriesToFucusPresentOfThatTypeIfItIsIInputElement() {
            _viewModel.OpenModule<InputElement>();
            var inputElement = (InputElement) _viewModel.Modules.Single();
            _viewModel.OpenModule<OtherInputElement>();
            _viewModel.OpenModuleCommand.Execute(typeof(InputElement));
            Assert.IsTrue(inputElement.Focused);
        }

        [Test]
        public void PasteModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.PasteModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.PasteCommand.CanExecute(null)).Returns(false);
            _viewModel.PasteModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.PasteCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.PasteCommand.CanExecute(null)).Returns(true);
            _viewModel.PasteModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.PasteCommand.Execute(null), Times.Once());
        }

        [Test]
        public void ReloadModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.ReloadModuleCommand.Execute(null));
            var viewModelStub = new Mock<IModuleViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.ReloadCommand.CanExecute(null)).Returns(false);
            _viewModel.ReloadModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.ReloadCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.ReloadCommand.CanExecute(null)).Returns(true);
            _viewModel.ReloadModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.ReloadCommand.Execute(null), Times.Once());
        }

        [Test]
        public void SaveInvokesSaveOnAllModules() {
            var conParamsStub = new Mock<INpgConnectionParameters>();
            _viewModel.ConnectionParameters = conParamsStub.Object;
            _viewModel.OpenModuleCommand.Execute(typeof(Module));
            var module = (Module) _viewModel.Modules.Single(m => m is Module);
            _viewModel.OpenModuleCommand.Execute(typeof(AnotherModule));
            var anotherModule = (AnotherModule) _viewModel.Modules.Single(m => m is AnotherModule);
            _viewModel.Save();
            Assert.IsTrue(module.Saved);
            Assert.IsTrue(anotherModule.Saved);
        }

        [Test]
        public void SaveModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.SaveModuleCommand.Execute(null));
            var viewModelStub = new Mock<IModuleViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.SaveCommand.CanExecute(null)).Returns(false);
            _viewModel.SaveModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.SaveCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.SaveCommand.CanExecute(null)).Returns(true);
            _viewModel.SaveModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.SaveCommand.Execute(null), Times.Once());
        }

        [Test]
        public void SelectedModuleSetRaisesPropertyChanged() {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "SelectedModuleViewModel") raised = true;
            };
            var viewModelStub = new Mock<IModuleViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            Assert.IsTrue(raised);
            raised = false;
            _viewModel.SelectedModuleViewModel = null;
            Assert.IsTrue(raised);
        }

        [Test]
        [TestCaseSource("DatabasesCases")]
        public void TitleContainsDatabase(string database) {
            _viewModel.ConnectionParameters = new NpgConnectionParameters {Database = database};
            Assert.IsTrue(_viewModel.Title.Contains(database));
        }

        [Test]
        public void UnselectModuleCommandTest() {
            Assert.DoesNotThrow(() => _viewModel.UnselectModuleCommand.Execute(null));
            var viewModelStub = new Mock<ICollectionViewModel>();
            _viewModel.SelectedModuleViewModel = viewModelStub.Object;
            viewModelStub.Setup(v => v.UnselectCommand.CanExecute(null)).Returns(false);
            _viewModel.UnselectModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.UnselectCommand.Execute(null), Times.Never());
            viewModelStub.Setup(v => v.UnselectCommand.CanExecute(null)).Returns(true);
            _viewModel.UnselectModuleCommand.Execute(null);
            viewModelStub.Verify(v => v.UnselectCommand.Execute(null), Times.Once());
        }
    }
}