using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebBrowserTest
{
    public sealed class MainViewModel : NotifyObject
    {
        public MainViewModel()
        {
            XWebId = "800000001844";
            AuthKey = "FVcicV8a6M0dPsGjTJFmrVd4YQlBj1NE";
            TerminalId = "80022895";
            Industry = "RETAIL";
            Amount = 1;
        }

        private string authKey;
        public string AuthKey
        {
            get { return authKey; }
            set { Set(ref authKey, value); }
        }

        private string terminalId;
        public string TerminalId
        {
            get { return terminalId; }
            set { Set(ref terminalId, value); }
        }

        private string xWebId;
        public string XWebId
        {
            get { return xWebId; }
            set { Set(ref xWebId, value); }
        }

        private string industry;
        public string Industry
        {
            get { return industry; }
            set { Set(ref industry, value); }
        }

        private decimal amount;
        public decimal Amount
        {
            get { return amount; }
            set { Set(ref amount, value); }
        }

        private string source;
        public string Source
        {
            get { return source; }
            set { Set(ref source, value); }
        }

        private ICommand charge;
        public ICommand Charge
        {
            get
            {
                if (charge == null)
                {
                    charge = new RelayCommand(ex =>
                    {
                        var charger = new CardCharger();
                        charger.HandleCharge(this);

                    });
                }
                return charge;
            }
        }
    }

    public abstract class NotifyObject : INotifyPropertyChanged
    {
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var shouldFire = !Equals(field, value);
            field = value;

            // ReSharper disable once ExplicitCallerInfoArgument
            if (shouldFire) OnPropertyChanged(propertyName);
            return shouldFire;
        }

        protected bool Set<T>(T initialValue, Func<T> setter, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(initialValue, setter.Invoke()))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
        protected bool Set(double? initialValue, Func<double?> setter, double tolerance, [CallerMemberName] string propertyName = null)
        {
            var newValue = setter.Invoke();
            if (initialValue == null ? newValue != null : newValue == null || Math.Abs(initialValue.Value - newValue.Value) > tolerance)
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class RelayCommand : ICommand
    {
        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            this.execute = execute;
            this.canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return canExecute == null || canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            execute(parameters);
        }
    }

}
