using FluentAssertions;
using Functional.Maybe.Just;
using NodaTime;
using NSubstitute;
using NUnit.Framework;
using ProductionCode.Customers;
using ProductionCode.Lib;
using ProductionCode.Loans;
using ProductionCode.Orders;
using System;
using System.Linq;

namespace UnitTests.Orders.Homework
{
  public class LoanOrderServiceTest_Homework
  {
    private IMongoDbAccessor _mongoDbAccessor = default!;
    private IPostgresAccessor _postgresAccessor = default!;
    private LoanOrderService _loanOrderService = default!;

    [SetUp]
    public void Setup()
    {
      _mongoDbAccessor = Substitute.For<IMongoDbAccessor>();
      _postgresAccessor = Substitute.For<IPostgresAccessor>();
      _loanOrderService = new LoanOrderService(_postgresAccessor, _mongoDbAccessor);
    }

    [Test]
    public void ShouldCreateStudentLoan()
    {
      var customer = AStudent();

      var loanOrderAssert = new LoanOrderAssert(_loanOrderService.StudentLoanOrder(customer));

      loanOrderAssert.BeCorrectForStudent();
    }

    [Test]
    public void ShouldThrowExceptionWhenOrderLoanIsOrderedNotForStudent()
    {
      Action orderLoan = () => _loanOrderService.StudentLoanOrder(ACustomer());

      orderLoan.Should()
        .Throw<InvalidOperationException>()
        .WithMessage("Cannot order student loan if pl.smarttesting.customer is not a student.");
    }

    [Test]
    public void ShouldGetPromotionDiscoutFromMongoDbAccessorOnceWhenCreateStudentLoan()
    {
      _loanOrderService.StudentLoanOrder(AStudent());

      _mongoDbAccessor.Received(1).GetPromotionDiscount("Student Promo");
    }

    [Test]
    public void ShouldUpdatePromotionStatisitcsOnceWhenCreateStudentLoan()
    {
      _loanOrderService.StudentLoanOrder(AStudent());

      _postgresAccessor.Received(1).UpdatePromotionStatistics("Student Promo");
    }

    [Test]
    public void LoadForStudentShouldHaveOnePromotion()
    {
      _mongoDbAccessor.GetPromotionDiscount("Student Promo").Returns(10);

      var orderLoan = new LoanOrderAssert(_loanOrderService.StudentLoanOrder(AStudent()));

      orderLoan.HavePromotion(1)
        .HavePromotionDiscount(0, 10);
    }

    private Customer AStudent()
    {
      var person = APerson();
      person.Student();

      return new Customer(
        Guid.NewGuid(),
        person);
    }

    private Customer ACustomer()
    {
      return new Customer(Guid.NewGuid(), APerson());
    }

    private Person APerson()
    {
      return new Person(
       "Jan",
       "Kowalski",
         new LocalDate(1996, 8, 28).Just(),
         Gender.Male,
         "96082812079");
    }
  }

  public class LoanOrderAssert
  {
    private readonly LoanOrder _loanOrder;

    public LoanOrderAssert(LoanOrder loanOrder)
    {
      _loanOrder = loanOrder;
    }

    public LoanOrderAssert BeCorrectForStudent()
    {
      return BeRegisteredToday()
        .HaveLoanType(LoanType.Student)
        .HaveComission(200);
    }

    public LoanOrderAssert BeRegisteredToday()
    {
      _loanOrder.OrderDate.Should().Be(Clocks.ZonedUtc.GetCurrentDate());
      return this;
    }

    public LoanOrderAssert HaveComission(decimal comission)
    {
      _loanOrder.Commission.Should().Be(comission);
      return this;
    }

    public LoanOrderAssert HaveLoanType(LoanType loanType)
    {
      _loanOrder.Type.Should().Be(loanType);
      return this;
    }

    public LoanOrderAssert HavePromotion(int promotionCount)
    {
      _loanOrder.Promotions.Count.Should().Be(promotionCount);
      return this;
    }

    public LoanOrderAssert HavePromotionDiscount(int position, decimal dictount)
    {
      _loanOrder.Promotions.ElementAt(position).Discount.Should().Be(dictount);
      return this;
    }
  }
} 
