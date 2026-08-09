"use client";

import {
  useEffect,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

import Uploader from '@components/atoms/defaultElements/uploader';
import {
  CloseIcon,
  PlusIcon,
} from '@components/atoms/iconComponents';

export interface ImagesInputProps {
  onChange: any;
  defaultValue?: { id: number; file: undefined | File; isMain: boolean }[];
}

export default function ImagesInput({
  onChange,
  defaultValue = [{ id: 1, file: undefined, isMain: true }],
}: ImagesInputProps) {
  const t = useTranslations('common');
  const [imageArray, setImageArray] =
    useState<{ id: number; file: undefined | File; isMain: boolean }[]>(
      defaultValue,
    );
  const addImageRow = (e: any) => {
    e.stopPropagation();
    e.preventDefault();
    setImageArray([
      ...imageArray,
      { id: imageArray.length + 1, file: undefined, isMain: false },
    ]);
  };

  const updateImage = (id: number, file: any) => {
    setImageArray((prev) =>
      prev.map((img) => (img.id === id ? { ...img, file } : img))
    );
  };

const removeImage = (e: React.MouseEvent, id: number) => {
  e.stopPropagation();
  e.preventDefault();
  
  setImageArray(prev => {
    const arr = prev.filter(img => img.id !== id);
    if (!arr.some(img => img.isMain) && arr.length > 0) {
      return arr.map((img, idx) => ({
        ...img,
        isMain: idx === 0
      }));
    }
    return arr;
  });
};

 const setMainImage = (targetId: number) => {
  setImageArray(prev =>
    prev.map(img => ({
      ...img,
      isMain: img.id === targetId
    }))
  );
};
 useEffect(()=>{
  onChange(imageArray)
 },[imageArray])
  return (
    <>
      {imageArray.map(
        (
          item: { id: number; file: undefined | File; isMain: boolean },
          index: number,
        ) => (
          <div
            key={item.id}
            className="flex items-center gap-4 pb-2 rounded-lg h-12"
          >
            <Uploader
              className="!min-h-0 !h-full"
              onChange={(file) => {
                updateImage(item.id, file);
              }}
            />
            <label className="flex items-center gap-2">
              <input
                type="radio"
                name="mainImage"
                checked={item.isMain}
                onChange={() => setMainImage(item.id)}
                className="w-4 h-4 text-blue-600"
              />
              <span>{t('primary')}</span> {index}
            </label>

            {imageArray.length > 1 && (
              <button
                type="button"
                onClick={(e) => removeImage(e, item.id)}
                className="bg-white border rounded-full text-red-600 text-sm"
              >
                <CloseIcon config={{ size: 16 }} />
              </button>
            )}
          </div>
        ),
      )}

      <button
        type="button"
        onClick={addImageRow}
        className="bg-white p-2 border border-blue-600 rounded-full text-blue-600 text-sm"
      >
        <PlusIcon />
      </button>
    </>
  );
}
