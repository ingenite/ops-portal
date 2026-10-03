import { useState } from "react";

type CreateWorkItemFormProps = {
  onCreate: (title: string) => Promise<void>;
};

export function CreateWorkItemForm({ onCreate }: CreateWorkItemFormProps) {
  const [title, setTitle] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSubmit(event: React.SyntheticEvent) {
    event.preventDefault();

    const trimmedTitle = title.trim();
    if (!trimmedTitle) {
      return;
    }

    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await onCreate(trimmedTitle);
      setTitle("");
    } catch (error: unknown) {
      setErrorMessage(
        error instanceof Error ? error.message : "An unknown error occurred",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="create-work-item-form" onSubmit={handleSubmit}>
      <div className="create-work-item-fields">
        <label className="title-field">
          <span>New WorkItem Title:</span>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={isSubmitting}
          />
        </label>

        <button
          className="primary-button"
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Creating..." : "Create"}
        </button>
      </div>

      {errorMessage && <p role="alert">{errorMessage}</p>}
    </form>
  );
}
