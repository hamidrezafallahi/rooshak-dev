'use client';

import React, { useEffect, useRef, useState } from 'react';

import { useParams, useRouter } from 'next/navigation';

import { useGetConditionallyMutation } from '@services/base';

import {
  Switch,
  SwitchProps,
} from '../switch';

interface IProps extends Omit<SwitchProps, 'id' | 'checked' | 'onChange'> {
  id: number;
  checked: boolean;
}

function ActiveComponent({ id, checked, ...props }: IProps) {
  const params = useParams()!;
  const router = useRouter();
  const [active, { isLoading }] = useGetConditionallyMutation();
  const [isActive, setIsActive] = useState(() => Boolean(checked));
  const pendingRef = useRef(false);
  const isActiveRef = useRef(isActive);

  useEffect(() => {
    isActiveRef.current = isActive;
  }, [isActive]);

  useEffect(() => {
    if (!pendingRef.current) {
      setIsActive(Boolean(checked));
    }
  }, [checked]);

  const handleSwitchChange = async () => {
    if (pendingRef.current || isLoading) return;

    const previous = isActiveRef.current;
    const target = !previous;

    pendingRef.current = true;
    isActiveRef.current = target;
    setIsActive(target);

    try {
      const res = await active({
        url: `/${params.field}/active`,
        body: {
          id: Number(id),
          isActive: target,
        },
        method: 'PUT',
      });

      // A refusal can also come as a normal answer with isSuccess false.
      if (('error' in res && res.error) || res.data?.isSuccess === false) {
        isActiveRef.current = previous;
        setIsActive(previous);
        return;
      }

      router.refresh();
    } catch (error) {
      isActiveRef.current = previous;
      setIsActive(previous);
      console.error(error);
    } finally {
      pendingRef.current = false;
    }
  };

  return (
    <Switch
      {...props}
      checked={isActive}
      disabled={isLoading || props.disabled}
      onChange={handleSwitchChange}
    />
  );
}

export default ActiveComponent;
