using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSuite.Domain.Constants;

namespace FinancialSuite.Domain.Interfaces
{
    public interface IProjectAuthorizationService
    {
        bool Authorize(Project project, ResourceOperation resourceOperation);
    }
}
