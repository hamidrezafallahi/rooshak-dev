import { IAddress } from '@components/templates/shoppingCartTemplate/type';
import { IPaymentMethod } from '@models/paymentMethod';
import {
  ICartProduct,
  SynchronousResponse,
} from '@models/product';
import { IShippingMethod } from '@models/shippingMethod';
import { IPromo } from '@models/shoppingCart';
import {
  createSlice,
  PayloadAction,
} from '@reduxjs/toolkit';

import { IShoppingCart } from './type';

const initialState: IShoppingCart = {
  products: [],
  totalPrice: 0,
  totalDiscount: 0,
  finalTotal: 0,
  address: null,
  shippingMethod: undefined,
  discountCodeAmount: 0,
  paymentMethod: undefined,
  promoCode: "",
};

const calculateTotals = (state: IShoppingCart) => {
  let totalPrice = 0;
  let totalDiscount = 0;

  state.products.forEach((p) => {
    const quantity = p.quantity || 1;
    const discountAmount = p.discountIsPercent
      ? (p.price * p.discountAmount) / 100
      : p.discountAmount || 0;

    totalPrice += p.price * quantity;
    totalDiscount += discountAmount * quantity;
  });

  state.totalPrice = totalPrice;
  state.totalDiscount = totalDiscount;
  state.finalTotal = totalPrice - totalDiscount;
};
const calculateTotalsFromPromotions = (state: IShoppingCart) => {
  let totalPrice = 0;
  let totalDiscount = 0;
  state.products.forEach((p) => {
    const quantity = p.quantity || 1;
    const discountAmount = p.discountAmount;
    totalPrice += p.price * quantity;
    totalDiscount += discountAmount * quantity;
  });
  state.totalPrice = totalPrice;
  state.totalDiscount = totalDiscount;
  state.finalTotal = totalPrice - totalDiscount;
};

export const ShoppingCartReducer = createSlice({
  name: "shoppingCartSlice",
  initialState,
  reducers: {
    addToCart: (state, action: PayloadAction<{ product: ICartProduct }>) => {
      const { product } = action.payload;
      const exist = state.products.find(
        (item) =>
          item.id === product.id &&
          item.productOfferId === product.productOfferId,
      );
      if (exist) {
        exist.quantity = (exist.quantity || 1) + 1;
      } else {
        state.products.push({ ...product, quantity: 1 });
      }
      calculateTotals(state);
    },
    synchronousCart: (state, action: PayloadAction<SynchronousResponse>) => {
      const { items, finalTotal, totalDiscount, totalPrice } = action.payload;
      // Keyed by offer: two variants of one product (e.g. 2.5x and 3.5x) are separate lines.
      const dbMap = new Map(items.map((ci) => [ci.productOfferId, ci]));
      const updatedProducts = state.products
        .filter((product) => dbMap.has(product.productOfferId))
        .map((product) => {
          const dbItem = dbMap.get(product.productOfferId)!;
          return {
            ...product,
            ...{
              name: dbItem.name,
              discountAmount: dbItem.discountAmount,
              finalPrice: dbItem.finalPrice,
              mainImage: dbItem.mainImage,
              price: dbItem.basePrice,
              quantity: dbItem.quantity,
              cartItemId: dbItem.cartItemId,
            },
          };
        });

      items.forEach((dbItem) => {
        const exists = updatedProducts.some((p) => p.productOfferId === dbItem.productOfferId);
        if (!exists) {
          updatedProducts.push({
            id: dbItem.productId,
            productOfferId: dbItem.productOfferId,
            name: dbItem.name,
            description: dbItem.description,
            price: dbItem.basePrice,
            discountAmount: dbItem.discountAmount,
            discountIsPercent: false,
            finalPrice: dbItem.finalPrice,
            mainImage: dbItem.mainImage,
            quantity: dbItem.quantity,
            cartItemId: dbItem.cartItemId,
          });
        }
      });

       state.products = updatedProducts;
      state.finalTotal = finalTotal;
      state.totalDiscount = totalDiscount;
      state.totalPrice = totalPrice;
    },

    increaseToCart: (
      state,
      action: PayloadAction<{ id: number; productOfferId: number }>,
    ) => {
      const { id, productOfferId } = action.payload;
      const item = state.products.find(
        (p) => p.id === id && p.productOfferId === productOfferId,
      );
      if (!item) return;
      item.quantity = (item.quantity || 1) + 1;
      calculateTotals(state);
    },

    decreaseFromCart: (
      state,
      action: PayloadAction<{ id: number; productOfferId: number }>,
    ) => {
      const { id, productOfferId } = action.payload;

      const item = state.products.find(
        (p) => p.id === id && p.productOfferId === productOfferId,
      );
      if (!item) return;

      if ((item.quantity || 1) > 1) {
        item.quantity!--;
      } else {
        // Remove only this line; other variants (offers) of the same product stay.
        state.products = state.products.filter(
          (p) => !(p.id === id && p.productOfferId === productOfferId),
        );
      }
      calculateTotals(state);
    },
    removeFromCart: (
      state,
      action: PayloadAction<{ id: number; productOfferId: number }>,
    ) => {
      const { id, productOfferId } = action.payload;

      // Remove only this line; other variants (offers) of the same product stay.
      state.products = state.products.filter(
        (p) => !(p.id === id && p.productOfferId === productOfferId),
      );
      calculateTotals(state);
    },
    setAddress: (
      state,
      action: PayloadAction<{ address: IAddress | null }>,
    ) => {
      state.address = action.payload.address;
    },
    setShippingMethod: (state, action: PayloadAction<IShippingMethod>) => {
      state.shippingMethod = action.payload;
    },
    setPaymentMethod: (state, action: PayloadAction<IPaymentMethod>) => {
      state.paymentMethod = action.payload;
    },
    clearShoppingCart: (state) => {
      state.products = [];
      state.totalPrice = 0;
      state.totalDiscount = 0;
      state.finalTotal = 0;
      state.address = null;
      state.shippingMethod = undefined;
    },
    setPromotionCode: (
      state,
      action: PayloadAction<{ promo: IPromo | null }>,
    ) => {
      if (action.payload.promo !== null) {
        const { isPercent, amount, code } = action.payload.promo;
        state.promoCode = code;
        const total = state.products.reduce(
          (acc: number, item) => acc + (item.finalPrice*item.quantity),
          0,
        );
         state.finalTotal = isPercent
          ? total - (total * amount) / 100
          : total - amount;
        state.discountCodeAmount = isPercent
          ? (total * amount) / 100
          :  amount;
      } else {
        state.promoCode = "";
        calculateTotalsFromPromotions(state);
        state.discountCodeAmount = 0;
      }
    },

    resetShoppingCart: () => initialState,
  },
});

export const {
  addToCart,
  synchronousCart,
  increaseToCart,
  decreaseFromCart,
  removeFromCart,
  setAddress,
  setShippingMethod,
  setPaymentMethod,
  setPromotionCode,
  resetShoppingCart,
  clearShoppingCart,
} = ShoppingCartReducer.actions;

export default ShoppingCartReducer.reducer;
