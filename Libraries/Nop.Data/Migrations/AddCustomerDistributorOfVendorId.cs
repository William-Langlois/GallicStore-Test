//using FluentMigrator;
//using Nop.Core.Domain.Customers;

//namespace Nop.Data.Migrations
//{
//    [NopMigration("2022/03/01 10:17:00:0101002")]
//    public class AddCustomerDistributorOfVendorId : AutoReversingMigration
//    {
//        #region Methods

//        public override void Up()
//        {
//            Create.Column(nameof(Customer.DistributorOfVendorId))
//            .OnTable(nameof(Customer))
//            .AsInt32()
//            .Nullable();
//        }

//        #endregion
//    }
//}
