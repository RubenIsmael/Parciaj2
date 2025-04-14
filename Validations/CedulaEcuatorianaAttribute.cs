using System;
using System.ComponentModel.DataAnnotations;

namespace San_Agustin_Final.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CedulaEcuatorianaAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            string cedula = value.ToString();

            // Verificar longitud
            if (cedula.Length != 10)
                return false;

            // Verificar que todos los caracteres sean dígitos
            foreach (char c in cedula)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Verificar el código de provincia (01 al 24)
            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
                return false;

            // Verificar el último dígito (dígito verificador)
            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int verificador = int.Parse(cedula.Substring(9, 1));
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula.Substring(i, 1)) * coeficientes[i];
                suma += (valor >= 10) ? valor - 9 : valor;
            }

            int digitoVerificador = (suma % 10 != 0) ? 10 - (suma % 10) : 0;

            return verificador == digitoVerificador;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (IsValid(value))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ?? "La cédula ecuatoriana no es válida.");
        }
    }
}