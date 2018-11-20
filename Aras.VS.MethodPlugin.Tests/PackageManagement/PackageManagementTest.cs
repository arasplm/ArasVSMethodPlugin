using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin.Tests.PackageManagement
{
    [TestFixture]
    public class PackageManagementTest
    {
        private PackageManager packageManager;
        IAuthenticationManager authManager;

        [SetUp]
        public void Init()
        {
            authManager = new AuthManagerStub();
            packageManager = new PackageManager(authManager);
        }

        [Test]
        public void Ctor_AuthenticationManager_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
            {
                // Act
                new PackageManager(null);
            }));
        }

        [Test]
        public void GetPackageDefinitionList_CallMethod_ShouldReturnEmptyList()
        {
            //Arrange

            //Act
            var expected = packageManager.GetPackageDefinitionList();

            //Assert
            Assert.AreEqual(expected.Count, 0);
        }


        [Test]
        public void GetPackageDefinitionByElementName_NameisNull_ShouldReturnEmptyString1111111()
        {
            //Arrange
            string name = null;
            
            
            //Act
            var expected = packageManager.GetPackageDefinitionByElementName(name);

            //Assert
            Assert.AreEqual(expected, string.Empty);
        }

        [Test]
        public void GetPackageDefinitionByElementName_NameisNull_ShouldReturnEmptyString()
        {
            //Arrange
            string name = null;

            //Act
            var expected = packageManager.GetPackageDefinitionByElementName(name);

            //Assert
            Assert.AreEqual(expected, string.Empty);
        }

        [Test]
        public void DeletePackageElementByNameFromPackageDefinition_NameisNull_ShouldReturnFalse()
        {
            //Arrange
            string name = null;

            //Act
            var expected = packageManager.DeletePackageElementByNameFromPackageDefinition(name);

            //Assert
            Assert.IsFalse(expected);
        }

        [Test]
        public void AddPackageElementToPackageDefinition()
        {
            //Arrange
            string name = null;
            string id = null;
            string packageName = null;

            //Act
            var testDelegate = new TestDelegate(() => packageManager.AddPackageElementToPackageDefinition(id, name, packageName));

            //Assert
            Assert.DoesNotThrow(testDelegate);
        }









    }
}
