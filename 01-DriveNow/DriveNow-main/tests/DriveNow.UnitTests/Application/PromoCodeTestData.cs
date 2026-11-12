
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Application.Pricing;

public class PromoCodeTestData : TheoryData<
    string, // represent promo code name
    PromoCode?, // actual object from DB 
    int, // days of rent
    VehicleCategory, // allowed for which vehicle type
    string?> // error msg
{
    public PromoCodeTestData()
    {
        // A valid promo code should be accepted.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                DiscountPercent = 15,
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                AllowedCategories = null,
                MaxUses = 100,
                UsedCount = 3
            },
            2,
            VehicleCategory.Economy,
            null);

        // A missing promo code should return the not-found message.
        Add(
            "UNKNOWN",
            null,
            2,
            VehicleCategory.Economy,
            "Promo code 'UNKNOWN' does not exist");

        // A promo code used before its start date should be rejected.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 13),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' has expired");

        // A promo code used after its end date should be rejected.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 11, 11),
                MinDays = 1,
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' has expired");

        // Rental duration must meet the promo minimum.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 5,
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' requires at least 5 days");

        // The vehicle category must be allowed by the promo.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                AllowedCategories = new List<VehicleCategory>
                {
                    VehicleCategory.Luxury
                },
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' is not valid for Economy");

        // A promo code cannot be used after reaching MaxUses.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                MaxUses = 10,
                UsedCount = 10
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' has been fully used");

        // First failure wins:
        // Expiry must be checked before minimum rental days.
        Add(
            "SAVE15",
            new PromoCode
            {
                Code = "SAVE15",
                ValidFrom = new DateTime(2026, 11, 13),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 5,
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            "Promo code 'SAVE15' has expired");
    }
}
