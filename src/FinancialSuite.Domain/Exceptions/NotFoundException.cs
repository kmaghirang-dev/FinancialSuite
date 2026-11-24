using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Exceptions
{
    public class NotFoundException(string resourceType,string resourceIdentifier) : Exception($"The {resourceType} with ID {resourceIdentifier} was not found.")
    {
    }
}
