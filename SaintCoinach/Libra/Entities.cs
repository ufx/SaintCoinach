using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SaintCoinach.Libra {
    public partial class Entities {
        public Entities(string connectionString) : base(GetOptions(connectionString)) { }

        private static DbContextOptions GetOptions(string connectionString) {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }
    }
}
