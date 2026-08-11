import { useState } from "react";
import { profilesApi } from "../../../../../services/api/ProfilesApi.ts";
import { OfficeInputFilter } from "../../../Shared/Inputs/OfficeInputFilter";
import { OfficeAddress } from "../../../specific/offices/OfficeCompactCard";
import { AlertMessage } from "./AccountProfileCard";

export function ReceptionistCreateForm({
  accountId,
  onSuccess,
}: {
  accountId: string;
  onSuccess: () => void;
}) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [office, setOffice] = useState<{ id: string } | null>(null);
  const [actionMessage, setActionMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setActionMessage(null);

    if (!office) {
      setActionMessage({ type: "error", text: "Office is not selected" });
      return;
    }

    try {
      const result = await profilesApi.createReceptionist(accountId, office.id);
      if (result.type === "ok") {
        setActionMessage({
          type: "success",
          text: "Receptionist profile created!",
        });
        onSuccess();
      } else {
        setActionMessage({
          type: "error",
          text: result.error?.title || "Failed to create profile",
        });
      }
    } catch {
      setActionMessage({ type: "error", text: "An unexpected error occurred" });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="account-details-card profile-form-card">
      <h3>Create Receptionist Profile</h3>
      {actionMessage && (
        <AlertMessage message={actionMessage.text} type={actionMessage.type} />
      )}
      <form className="details-form" onSubmit={handleSubmit}>
        <OfficeInputFilter valueId={office?.id ?? null} onChange={setOffice} />
        <div className="form-actions">
          <button type="submit" className="submit-btn" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create Profile"}
          </button>
        </div>
      </form>
    </div>
  );
}

export function ReceptionistCard({
  accountId,
  initialOfficeId,
  onUpdateSuccess,
}: {
  accountId: string;
  initialOfficeId: string;
  onUpdateSuccess: () => void;
}) {
  const [isEditing, setIsEditing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [officeId, setOfficeId] = useState(initialOfficeId);
  const [actionMessage, setActionMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  const handleUpdate = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setActionMessage(null);

    try {
      const result = await profilesApi.updateReceptionist(accountId, officeId);
      if (result.type === "ok") {
        setIsEditing(false);
        onUpdateSuccess();
      } else {
        setActionMessage({
          type: "error",
          text: result.error?.title || "Failed to update profile",
        });
      }
    } catch {
      setActionMessage({ type: "error", text: "An unexpected error occurred" });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="account-details-card profile-form-card">
      <header className="details-header">
        <h3>Receptionist Profile</h3>
        {!isEditing && (
          <button className="edit-btn" onClick={() => setIsEditing(true)}>
            Edit
          </button>
        )}
      </header>

      {actionMessage && (
        <AlertMessage message={actionMessage.text} type={actionMessage.type} />
      )}

      {isEditing ? (
        <form className="details-form" onSubmit={handleUpdate}>
          <div className="form-group">
            <label>Office ID</label>
            <OfficeInputFilter
              valueId={officeId}
              onChange={(office) => setOfficeId(office?.id ?? "")}
            />
          </div>
          <div className="form-actions">
            <button
              type="submit"
              className="submit-btn"
              disabled={isSubmitting}
            >
              Save
            </button>
            <button
              type="button"
              className="cancel-btn"
              onClick={() => setIsEditing(false)}
              disabled={isSubmitting}
            >
              Cancel
            </button>
          </div>
        </form>
      ) : (
        <div className="info-grid">
          <div className="info-item">
            <span className="label">Office ID</span>
            <OfficeAddress officeId={initialOfficeId} />
          </div>
        </div>
      )}
    </div>
  );
}
