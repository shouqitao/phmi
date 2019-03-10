using System;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.Users;

namespace PHmiClient.Controls.Win {
    [TemplatePart(Name = "pswb", Type = typeof(PasswordBox))]
    public class SetPasswordDialog : DialogBase {
        private readonly IUsers _users;
        private PasswordBox _pswb;

        static SetPasswordDialog() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SetPasswordDialog),
                new FrameworkPropertyMetadata(typeof(SetPasswordDialog)));
        }

        public SetPasswordDialog(IUsers users, User user) {
            _users = users;
            User = user;
        }

        public User User { get; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            _pswb = (PasswordBox) GetTemplateChild("pswb");
            if (_pswb != null) _pswb.Focus();
            _pswb = (PasswordBox) GetTemplateChild("pswb");
        }

        protected override void OkCommandExecuted(object obj) {
            StartLoading();
            _users.SetPassword(User.Id, _pswb.Password, r => Dispatcher.Invoke(new Action(() => {
                EndLoading(r);
                if (!r) _pswb.Focus();
            })));
        }
    }
}