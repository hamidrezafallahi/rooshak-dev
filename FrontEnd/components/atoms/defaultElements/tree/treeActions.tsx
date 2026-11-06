import React, { useEffect, useState } from 'react';

import {
  EditIcon,
  PlusIcon2,
  SpinnerIcon,
  TrashbinIcon,
} from '@components/atoms/iconComponents';
import { useCUDDataMutation } from '@services/base';
import { showErrorToast } from '@utils/core';

import { Switch } from '../switch';
import { ITreeContext } from './';

interface TreeActionsProps {
  node: ITreeContext;
  active: boolean;
  endPoint: string;
  onClick: (e: ITreeContext | { parentCategoryId: number }) => void;
}
export default function TreeActions({ ...props }: TreeActionsProps) {
  const { node, endPoint, onClick } = props;
  return (
    <div className="relative flex flex-wrap justify-end items-center gap-1.5">
      <EditCategory
        onClick={() => {
          onClick(node);
        }}
      />
      <DeleteCategory endpoint={endPoint} id={node.id} />
      <ActiveCategory endpoint={endPoint} id={node.id} active={node.isActive} />
      <AddSubCategory
        onClick={() => {
          onClick({ parentCategoryId: node.id });
        }}
      />
    </div>
  );
}
const EditCategory = ({ onClick }: { onClick: () => void }) => {
  return (
    <button type="button" onClick={onClick} className="admin-icon-btn !w-8 !h-8">
      <EditIcon />
    </button>
  );
};
const DeleteCategory = ({ id, endpoint }: { id: number; endpoint: string }) => {
  const [deleted, setDeleted] = useState(false);
  const [deleteApi, { isLoading }] = useCUDDataMutation();
  const deleteHandler = async () => {
    try {
      const res = await deleteApi({
        url: `/${endpoint + "/" + id}`,
        method: "DELETE",
      }).unwrap();
      if (res) {
        setDeleted(res.isSuccess);
      }
    } catch {
      // baseQueryByToken already showed the reason.
    }
  };
  return (
    <button
      type="button"
      disabled={deleted}
      onClick={deleteHandler}
      className="admin-icon-btn admin-icon-btn-danger !w-8 !h-8"
    >
      {isLoading ? (
        <SpinnerIcon config={{ fill: "#999", stroke: "#444" }} />
      ) : deleted ? (
        <TrashbinIcon config={{ stroke: "#888" }} />
      ) : (
        <TrashbinIcon config={{ stroke: "var(--error-color)" }} />
      )}
    </button>
  );
};
const ActiveCategory = ({
  id,
  endpoint,
  active,
}: {
  id: number;
  endpoint: string;
  active: boolean;
}) => {
  const [isActive, setIsActive] = useState(Boolean(active));
  const [ActiveApi, { isLoading }] = useCUDDataMutation();

  useEffect(() => {
    setIsActive(Boolean(active));
  }, [active]);

  const activeHandler = async (next: boolean) => {
    const previous = isActive;
    const target = next === previous ? !previous : next;
    setIsActive(target);

    try {
      const res = await ActiveApi({
        url: `/${endpoint}/active`,
        body: {
          id: Number(id),
          isActive: target,
        },
        method: 'PUT',
      }).unwrap();

      if (!res.isSuccess) {
        setIsActive(previous);
        showErrorToast(res.error);
      }
    } catch {
      // baseQueryByToken already showed the reason.
      setIsActive(previous);
    }
  };

  return (
    <Switch
      onChange={activeHandler}
      checked={isActive}
      disabled={isLoading}
    />
  );
};

const AddSubCategory = ({ onClick }: { onClick: () => void }) => {
  return (
    <button type="button" onClick={onClick} className="admin-icon-btn !w-8 !h-8">
      <PlusIcon2 />
    </button>
  );
};
