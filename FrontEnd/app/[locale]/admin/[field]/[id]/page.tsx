import FormGenerator from '@components/organisms/formGenerator';
import Product3DAdminPanel from '@components/organisms/productOrganisms/product3DAdminPanel';
import {
  getById,
  getFormConfigByEntityName,
} from '@lib/getAll';

export const dynamic = "force-dynamic";

const positive = (v: unknown) =>
  typeof v === 'number' && Number.isFinite(v) && v > 0 ? v : null;

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
          {/* Product dimensions are stored in cm; they pre-fill the 3D "real size" field. */}
          <Product3DAdminPanel
            productId={productId}
            productSize={{
              width: positive(defaultValues?.width),
              height: positive(defaultValues?.height),
              depth: positive(defaultValues?.depth),
            }}
          />
        </div>
      )}
    </>
  );
}
