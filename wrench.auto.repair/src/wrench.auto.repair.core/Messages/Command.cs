using FluentValidation.Results;
using MediatR;
using wrench.auto.repair.core.Errors;

namespace wrench.auto.repair.core.Messages
{
    public abstract class Command : Message, IRequest<Result>
    {
        public DateTime Timestamp { get; private set; }
        public ValidationResult ValidationResult { get; set; }

        protected Command()
        {
            Timestamp = DateTime.UtcNow;
        }

        public virtual bool EhValido()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ObterErros()
        {
            return ValidationResult.Errors.Select(e => e.ErrorMessage);
        }
    }
}
