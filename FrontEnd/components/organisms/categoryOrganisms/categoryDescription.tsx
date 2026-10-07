import React from 'react';

export  function CategoryDescription({desc}:{desc:string}) {
  return (
    <div className='border-s-2 border-store-strong bg-store-muted p-4 text-sm leading-7'>{desc}</div>
  )
}
