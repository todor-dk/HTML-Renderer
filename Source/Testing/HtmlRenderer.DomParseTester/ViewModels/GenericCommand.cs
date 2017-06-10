using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HtmlRenderer.DomParseTester.ViewModels
{
    /// <summary>
    /// An <see cref="ICommand"/> implementation that raises events to test 
    /// if the command can execute and when the command is executed.
    /// <para/>
    /// The command can also be initialized using delegates.
    /// </summary>
    public class GenericCommand : ICommand
    {
        /// <summary>
        /// Create and initialize a new GenericCommand.
        /// </summary>
        public GenericCommand()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCommand"/> class.
        /// </summary>
        /// <param name="execute">A delegate to be invoked when the action is executed.</param>
        public GenericCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCommand"/> class.
        /// </summary>
        /// <param name="execute">A delegate to be invoked when the action is executed.</param>
        /// <param name="canExecute">A delegate to be invoked to determine if the action can execute.</param>
        public GenericCommand(Action execute, Func<bool> canExecute)
        {
            if (execute != null)
                this.Execute += (s, e) => execute();
            if (canExecute != null)
                this.CanExecute += (s, e) => e.CanExecute = canExecute();
        }

        /// <summary>
        /// Occurs when the command initiates a check to determine whether 
        /// the command can be executed on the command target.
        /// </summary>
        public event EventHandler<CanExecuteEventArgs> CanExecute;

        /// <summary>
        /// Occurs when the command executes.
        /// </summary>
        public event EventHandler<ExecuteEventArgs> Execute;

        bool ICommand.CanExecute(object parameter)
        {
            EventHandler<CanExecuteEventArgs> evt = Volatile.Read(ref this.CanExecute);
            if (evt == null)
            {
                return true;
            }
            else
            {
                CanExecuteEventArgs args = new CanExecuteEventArgs(this);
                evt(this, args);
                return args.CanExecute;
            }
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(object parameter)
        {
            EventHandler<ExecuteEventArgs> evt = Volatile.Read(ref this.Execute);
            if (evt != null)
                evt(this, new ExecuteEventArgs(this));
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to update 
        /// whether or not the command should execute.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            EventHandler evt = Volatile.Read(ref this.CanExecuteChanged);
            if (evt != null)
                evt(this, EventArgs.Empty);
        }

        /// <summary>
        /// Provides data for the CanExecuted events.
        /// </summary>
        public sealed class CanExecuteEventArgs : EventArgs
        {
            internal CanExecuteEventArgs(GenericCommand command)
            {
                this.Command = command;
            }

            /// <summary>
            /// Gets or sets a value that indicates whether the GenericCommand
            //  associated with this event can be executed on the command target.
            /// </summary>
            public bool CanExecute { get; set; }

            /// <summary>
            /// Gets the command associated with this event.
            /// </summary>
            public GenericCommand Command { get; private set; }
        }

        /// <summary>
        /// Provides data for the Executed events.
        /// </summary>
        public sealed class ExecuteEventArgs : EventArgs
        {
            internal ExecuteEventArgs(GenericCommand command)
            {
                this.Command = command;
            }

            /// <summary>
            /// Gets the command associated with this event.
            /// </summary>
            public GenericCommand Command { get; private set; }
        }
    }
}
