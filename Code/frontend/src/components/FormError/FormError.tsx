interface FormErrorProps {
  message: string | null;
}

export function FormError({ message }: FormErrorProps) {
  if (!message) return null;
  return (
    <span className="text-red-400 text-sm bg-red-400/8 rounded px-2 py-1">
      {message}
    </span>
  );
}
