using FluentAssertions;
using Functional.Maybe.Just;
using NodaTime;
using NUnit.Framework;
using ProductionCode.Customers;
using ProductionCode.Verifier;
using ProductionCode.Verifier.Customers.Verification;
using System;

namespace UnitTests.Customers.Verification
{
  public class AgeVerificationTests_S
  {
    private IVerification service = new AgeVerification();

    [Test]
    public void verificationShouldNotPassForUnderagePerson()
    {
      Person personToVerify = PersonWithAge(17);

      var verificationResult = service.Passes(personToVerify);

      verificationResult.Should().BeFalse();
    }

    [Test]
    public void verificationShouldNotPassForToOldPerson()
    {
      Person personToVerify = PersonWithAge(100);

      var verificationResult = service.Passes(personToVerify);

      verificationResult.Should().BeFalse();
    }

    [Test]
    public void verificationShouldThrowExceptionForPersionWithNegativeAge()
    {
      Person personToVerify = PersonWithAge(-1);

      Action ageVerify = () =>  service.Passes(personToVerify);

      ageVerify.Should().Throw<InvalidOperationException>()
        .WithMessage("Age cannot be negative.");
    } 

    [Test]
    public void ShouldITestNullReferenceException()
    {
      var verificationResult = service.Passes(null); 
      //TODO?
      verificationResult.Should().BeFalse(); 
    }
  
    [TestCase(18)]
    [TestCase(49)]
    [TestCase(99)]
    public void verificationShouldPassWhenPersonIsAdult(int age)
    {
      var personToVerify = PersonWithAge(age);

      var verificationResult = service.Passes(personToVerify);

      verificationResult.Should().BeTrue();
    }

    private static Person PersonWithAge(int years)
    {
      var today = LocalDate.FromDateTime(DateTime.Now);
      var birthDate = today.Minus(Period.FromYears(years));
      return new Person(String.Empty, string.Empty, birthDate.Just(), Gender.Female, string.Empty);
    }
  }
}