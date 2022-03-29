//using FluentMigrator;
//using Nop.Core.Domain.Orders;

//namespace Nop.Data.Migrations
//{
//    [NopMigration("2022/03/01 10:15:00:0101001")]
//    public class AddShoppingCartItemVendorId : AutoReversingMigration
//    {
//        #region Methods

//        public override void Up()
//        {
//            Create.Column(nameof(ShoppingCartItem.VendorId))
//            .OnTable(nameof(ShoppingCartItem))
//            .AsInt32()
//            .Nullable();
//        }

//        #endregion
//    }
//}
