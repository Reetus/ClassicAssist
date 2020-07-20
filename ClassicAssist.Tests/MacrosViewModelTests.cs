//TODO
//using System.Linq;
//using ClassicAssist.Data.Macros;
//using ClassicAssist.UI.ViewModels;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace ClassicAssist.Tests
//{
//    [TestClass]
//    public class MacrosViewModelTests
//    {
//        private MacrosTabViewModel _model;

//        [TestInitialize]
//        public void Initialize()
//        {
//            _model = new MacrosTabViewModel();
//        }

//        [TestMethod]
//        public void NewCommandWillAddItem()
//        {
//            _model.NewMacroCommand.Execute( null );

//            Assert.AreEqual( 1, _model.Items.Count );

//            _model.Items.Clear();
//        }

//        [TestMethod]
//        public void RemoveCommandWillRemoveItem()
//        {
//            _model.NewMacroCommand.Execute( null );

//            MacroEntry item = _model.Items.FirstOrDefault();

//            _model.RemoveMacroCommand.Execute( item );

//            Assert.AreEqual( 0, _model.Items.Count );
//        }
//    }
//}