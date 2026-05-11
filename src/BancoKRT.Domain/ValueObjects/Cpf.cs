using System.Linq;
using System.Text;
using BancoKRT.Domain.Common;

namespace BancoKRT.Domain.ValueObjects
{
    public sealed record Cpf
    {
        public string Value { get; }

        private Cpf(string value)
        {
            Value = value;
        }

        public static DomainResult<Cpf> Create(string value)
        {
            var normalizedCpf = Normalize(value);

            if (string.IsNullOrWhiteSpace(normalizedCpf))
            {
                return DomainResult<Cpf>.Failure(
                    DomainErrorType.Validation,
                    "O CPF é obrigatório.");
            }

            if (normalizedCpf.Length != 11)
            {
                return DomainResult<Cpf>.Failure(
                    DomainErrorType.Validation,
                    "O CPF informado é inválido.");
            }

            if (normalizedCpf.Distinct().Count() == 1)
            {
                return DomainResult<Cpf>.Failure(
                    DomainErrorType.Validation,
                    "O CPF informado é inválido.");
            }

            if (!HasValidVerifierDigits(normalizedCpf))
            {
                return DomainResult<Cpf>.Failure(
                    DomainErrorType.Validation,
                    "O CPF informado é inválido.");
            }

            return DomainResult<Cpf>.Success(new Cpf(normalizedCpf));
        }

        public override string ToString()
        {
            return Value;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);

            foreach (var c in value)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static bool HasValidVerifierDigits(string normalizedCpf)
        {
            var firstDigit = CalculateVerifierDigit(normalizedCpf, 9);
            var secondDigit = CalculateVerifierDigit(normalizedCpf, 10);

            return normalizedCpf[9] - '0' == firstDigit
                && normalizedCpf[10] - '0' == secondDigit;
        }

        private static int CalculateVerifierDigit(string normalizedCpf, int length)
        {
            var sum = 0;
            var weight = length + 1;

            for (var index = 0; index < length; index++)
            {
                sum += (normalizedCpf[index] - '0') * weight;
                weight--;
            }

            var remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }
    }
}
