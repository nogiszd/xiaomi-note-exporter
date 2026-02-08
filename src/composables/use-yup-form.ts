import type { InferType, ObjectSchema } from "yup";
import { toTypedSchema } from "@vee-validate/yup";
import { useForm } from "vee-validate";

export function useYupForm<TSchema extends ObjectSchema<any>>(
  schema: TSchema,
  initialValues: Partial<InferType<TSchema>>,
) {
  return useForm<InferType<TSchema>>({
    validationSchema: toTypedSchema(schema),
    initialValues: initialValues as InferType<TSchema>,
  });
}
