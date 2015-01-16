using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using NewsHole.Data.Repositories;
using NHibernate;

namespace NewsHole.Tests.Data.Entities
{
    [TestClass]
    public class RepositoryTests
    {
        Repository<TestObject, TestKey> _testRepo;
        Mock<ISession> _testSession;
        Mock<ITransaction> _testTransaction;
        TestObject _testObj;
        TestKey _testKey;

        [TestInitialize]
        public void Initialize()
        {
            _testObj = new TestObject();
            _testKey = new TestKey();

            _testSession = new Mock<ISession>();
            _testTransaction = new Mock<ITransaction>();
            _testTransaction.Setup(x => x.Commit()).Verifiable();

            _testSession.Setup(x => x.BeginTransaction()).Returns(_testTransaction.Object);
        }

        [TestMethod]
        public void Test_Add_Saves_Item()
        {
            _testSession.Setup(x => x.Save(_testObj)).Verifiable();
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Add(_testObj);

            _testSession.Verify();
        }

        [TestMethod]
        public void Test_Add_Commits_Transaction()
        {
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Add(_testObj);

            _testTransaction.Verify();
        }

        [TestMethod]
        public void Test_Delete_Deletes_Item()
        {
            _testSession.Setup(x => x.Delete(_testObj)).Verifiable();
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Delete(_testObj);

            _testSession.Verify();
        }

        [TestMethod]
        public void Test_Delete_Commits_Transaction()
        {
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Delete(_testObj);

            _testTransaction.Verify();
        }

        [TestMethod]
        public void Test_Update_Updates_Item()
        {
            _testSession.Setup(x => x.Update(_testObj)).Verifiable();
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Update(_testObj);

            _testSession.Verify();
        }

        [TestMethod]
        public void Test_Update_Commits_Transaction()
        {
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            _testRepo.Update(_testObj);

            _testTransaction.Verify();
        }

        [TestMethod]
        public void Test_Get_Retrieves_TestObject()
        {
            _testSession.Setup(x => x.Get(typeof(TestObject), _testKey)).Returns(_testObj).Verifiable();
            _testRepo = new Repository<TestObject, TestKey>(_testSession.Object);
            var item = _testRepo.Get(_testKey);

            _testSession.Verify();
            Assert.AreSame(_testObj, item);
        }
    }

    internal class TestObject
    {
    }

    internal class TestKey
    {
    }
}
