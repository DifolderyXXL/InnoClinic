import {useState } from "react";
import {  type SpecializationDto, servicesApi } from "../../../../services/api/ServicesApi.ts";
import "./ServicesPage.css";

interface SpecializationsManagerModalProps {
    specializations: SpecializationDto[];
    onClose: () => void;
    onRefresh: () => void;
}

export function SpecializationsManagerModal({ specializations, onClose, onRefresh }: SpecializationsManagerModalProps) {
    const [newSpecName, setNewSpecName] = useState("");
    const [newSpecIsActive, setNewSpecIsActive] = useState(true);

    const [editingSpecId, setEditingSpecId] = useState<number | null>(null);
    const [editingSpecName, setEditingSpecName] = useState("");
    const [editingSpecIsActive, setEditingSpecIsActive] = useState(true);

    const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
    const [hasError, setHasError] = useState(false);

    const handleCreate = async () => {
        if (!newSpecName.trim()) {
            setHasError(true);
            return;
        }

        const result = await servicesApi.createSpecialization({
            specializationName: newSpecName.trim(),
            isActive: newSpecIsActive,
        });

        if (result?.type === "ok") {
            setNewSpecName("");
            setNewSpecIsActive(true);
            setHasError(false);
            onRefresh();
        } else {
            setHasError(true);
        }
    };

    const handleUpdate = async (specId: number) => {
        if (!editingSpecName.trim()) {
            setHasError(true);
            return;
        }

        const result = await servicesApi.updateSpecialization(specId, {
            id: specId,
            specializationName: editingSpecName.trim(),
            isActive: editingSpecIsActive,
        });

        if (result?.type === "ok") {
            setEditingSpecId(null);
            setHasError(false);
            onRefresh();
        } else {
            setHasError(true);
        }
    };

    const handleDelete = async (specId: number) => {
        if (confirmDeleteId !== specId) {
            setConfirmDeleteId(specId);
            return;
        }

        const result = await servicesApi.deleteSpecialization(specId);

        if (result?.type === "ok") {
            setConfirmDeleteId(null);
            setHasError(false);
            onRefresh();
        } else {
            setHasError(true);
            setConfirmDeleteId(null);
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                    <h3>Manage Specializations Pool</h3>
                    <button type="button" className="close-btn" onClick={onClose}>✕</button>
                </div>

                <div className="create-spec-form">
                    <input
                        type="text"
                        placeholder="New specialization name"
                        value={newSpecName}
                        onChange={(e) => {
                            setNewSpecName(e.target.value);
                            if (hasError) setHasError(false);
                        }}
                    />
                    <label className="checkbox-label">
                        <input
                            type="checkbox"
                            checked={newSpecIsActive}
                            onChange={(e) => setNewSpecIsActive(e.target.checked)}
                        />
                        Active
                    </label>
                    <button
                        type="button"
                        className={hasError ? "btn-error" : ""}
                        disabled={hasError}
                        onClick={handleCreate}
                    >
                        + Add to Pool
                    </button>
                </div>

                <div className="spec-list-pool">
                    {specializations.map((spec) => {
                        const specId = Number(spec.id);
                        const isEditing = editingSpecId === specId;
                        const isConfirmingDelete = confirmDeleteId === specId;

                        return (
                            <div key={spec.id} className="spec-pool-item">
                                {isEditing ? (
                                    <div className="edit-row">
                                        <input
                                            type="text"
                                            value={editingSpecName}
                                            onChange={(e) => {
                                                setEditingSpecName(e.target.value);
                                                if (hasError) setHasError(false);
                                            }}
                                        />
                                        <label className="checkbox-label">
                                            <input
                                                type="checkbox"
                                                checked={editingSpecIsActive}
                                                onChange={(e) => setEditingSpecIsActive(e.target.checked)}
                                            />
                                            Active
                                        </label>
                                        <button
                                            type="button"
                                            className={hasError ? "btn-error" : ""}
                                            disabled={hasError}
                                            onClick={() => handleUpdate(specId)}
                                        >
                                            Save
                                        </button>
                                        <button type="button" onClick={() => setEditingSpecId(null)}>
                                            Cancel
                                        </button>
                                    </div>
                                ) : (
                                    <>
                                        <div className="spec-info">
                                            <span>{spec.specializationName}</span>
                                            <span className={`service-status ${spec.isActive ? "active" : "inactive"}`}>
                                                {spec.isActive ? "Active" : "Inactive"}
                                            </span>
                                        </div>
                                        <div className="actions">
                                            <button
                                                type="button"
                                                onClick={() => {
                                                    setEditingSpecId(specId);
                                                    setEditingSpecName(spec.specializationName);
                                                    setEditingSpecIsActive(spec.isActive);
                                                }}
                                            >
                                                Edit
                                            </button>
                                            <button
                                                type="button"
                                                className="btn-delete"
                                                onClick={() => handleDelete(specId)}
                                            >
                                                {isConfirmingDelete ? "Confirm?" : "Delete"}
                                            </button>
                                            {isConfirmingDelete && (
                                                <button type="button" onClick={() => setConfirmDeleteId(null)}>
                                                    Cancel
                                                </button>
                                            )}
                                        </div>
                                    </>
                                )}
                            </div>
                        );
                    })}
                </div>
            </div>
        </div>
    );
}