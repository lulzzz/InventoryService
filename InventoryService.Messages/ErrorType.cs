﻿namespace InventoryService.Messages
{
    public enum ErrorType
    {
        Unknown = 0,
        NO_PRODUCT_ID_SPECIFIED,
        UNABLE_TO_READ_INV,
        RESERVATION_EXCEED_QUANTITY,
        HOLD_EXCEED_QUANTITY_FOR_HOLD,
        HOLD_EXCEED_QUANTITY_FOR_UPDATEQUANTITYANDHOLD,
        NEGATIVE_PURCHASE_FOR_PURCHASEFROMRESERVATION,
        PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMRESERVATION,
        PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMHOLD,
        NEGATIVE_PURCHASE_FOR_PURCHASEFROMHOLD,
        UNABLE_TO_UPDATE_INVENTORY_STORAGE
    }
}