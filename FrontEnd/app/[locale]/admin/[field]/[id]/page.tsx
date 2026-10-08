import FormGenerator from '@components/organisms/formGenerator';
import Product3DAdminPanel from '@components/organisms/productOrganisms/product3DAdminPanel';
import {
  getById,
  getFormConfigByEntityName,
} from '@lib/getAll';

export const dynamic = "force-dynamic";

// حذف PageParams و استفاده از نوع مستقیم
export default async function Page({
  params,
}: {
  params: Promise<{ id: string; field: string }>;
}) {
  const { field, id } = await params;
  const defaultValues: Record<string, unknown> = await getById(field, id);
  const res = await getFormConfigByEntityName(field);
  const productId = Number(id);
  const isProduct = field.toLowerCase() === 'products' && Number.isInteger(productId) && productId > 0;
  return (
    <>
      <FormGenerator entityFormConfig={res} defaultValues={defaultValues} />
      {isProduct && (
        <div className="admin-page">
          <Product3DAdminPanel productId={productId} />
        </div>
      )}
    </>
  );
}
