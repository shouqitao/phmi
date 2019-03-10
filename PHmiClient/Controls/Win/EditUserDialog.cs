using System;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.Loc;
using PHmiClient.Users;

namespace PHmiClient.Controls.Win {
    [TemplatePart(Name = "tbName", Type = typeof(TextBox))]
    [TemplatePart(Name = "tbError", Type = typeof(TextBlock))]
    public class EditUserDialog : DialogBase {
        private readonly IUsers _users;
        private TextBox _tb;
        private TextBlock _tbError;

        static EditUserDialog() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EditUserDialog),
                new FrameworkPropertyMetadata(typeof(EditUserDialog)));
        }

        public EditUserDialog(IUsers users, User user) {
            _users = users;
            User = new User();
            User.UpdateFrom(user);
        }

        public User User { get; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            _tb = (TextBox) GetTemplateChild("tbName");
            if (_tb != null) _tb.Focus();
            _tbError = (TextBlock) GetTemplateChild("tbError");
        }

        protected override void OkCommandExecuted(object obj) {
            StartLoading();
            _users.UpdateUser(User, r => Dispatcher.Invoke(new Action(() => {
                EndLoading(r == UpdateUserResult.Success);
                if (r != UpdateUserResult.Success) _tb.Focus();
                switch (r) {
                    case UpdateUserResult.NameConflict:
                        _tbError.Text = Res.NameConflict;
                        break;

                    case UpdateUserResult.UserNotFound:
                        _tbError.Text = Res.UserNotFound;
                        break;

                    case UpdateUserResult.NullValue:
                        _tbError.Text = Res.EmptyValue;
                        break;

                    default:
                        _tbError.Text = Res.Error;
                        break;
                }
            })));
        }
    }
}