import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { RequireRole, Roles } from "../../../../components/common/RequireRole.tsx";
import { type CategoryDto, type SpecializationDto, servicesApi } from "../../../../services/api/ServicesApi.ts";
import { groupBy } from "../../../../utilities/arrayUtils.ts";
import "./ServicesPage.css";
import type { ServiceDto } from "./ServicesPage.tsx";
import {SpecializationsManagerModal} from "./SpecializationsManagerModal.tsx";


interface ServiceCreateViewProps {
    categoryId: number;
    specializations: SpecializationDto[];
    defaultSpecializationId?: number;
    onSuccess: () => void;
    onCancel: () => void;
}

export function ServiceCreateView({
                                      categoryId,
                                      specializations,
                                      defaultSpecializationId,
                                      onSuccess,
                                      onCancel,
                                  }: ServiceCreateViewProps) {
    const [serviceName, setServiceName] = useState("");
    const [price, setPrice] = useState<number | "">("");
    const [isActive, setIsActive] = useState(true);
    const [selectedSpecId, setSelectedSpecId] = useState<number | "">(
        defaultSpecializationId ?? (specializations[0] ? Number(specializations[0].id) : "")
    );
    const [hasError, setHasError] = useState(false);

    const handleCreate = async () => {
        if (!serviceName.trim() || price === "" || selectedSpecId === "") {
            setHasError(true);
            return;
        }

        const payload = {
            serviceName: serviceName.trim(),
            price: Number(price),
            isActive,
            categoryId,
            specializationId: Number(selectedSpecId),
        };

        const result = await servicesApi.createService(payload);

        if (result?.type === "ok") {
            onSuccess();
        } else {
            setHasError(true);
        }
    };

    return (
        <div className="service-card creating">
            <input
                type="text"
                placeholder="Service name"
                value={serviceName}
                onChange={(e) => {
                    setServiceName(e.target.value);
                    if (hasError) setHasError(false);
                }}
            />
            <input
                type="number"
                placeholder="Price"
                value={price}
                onChange={(e) => {
                    const val = e.target.value;
                    setPrice(val === "" ? "" : Number(val));
                    if (hasError) setHasError(false);
                }}
            />
            <select
                value={selectedSpecId}
                onChange={(e) => {
                    setSelectedSpecId(e.target.value === "" ? "" : Number(e.target.value));
                    if (hasError) setHasError(false);
                }}
            >
                <option value="" disabled>Select Specialization</option>
                {specializations.map((spec) => (
                    <option key={spec.id} value={spec.id}>
                        {spec.specializationName} {spec.isActive ? "" : "(Inactive)"}
                    </option>
                ))}
            </select>
            <label className="checkbox-label">
                <input
                    type="checkbox"
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                />
                Active
            </label>
            <button
                type="button"
                className={hasError ? "btn-error" : ""}
                disabled={hasError}
                onClick={handleCreate}
            >
                Create
            </button>
            <button type="button" onClick={onCancel}>
                Cancel
            </button>
        </div>
    );
}

interface ServiceViewProps {
    service: ServiceDto;
    specializations: SpecializationDto[];
    onUpdateSuccess: () => void;
    onDeleteSuccess: () => void;
}

export function ServiceView({ service, specializations, onUpdateSuccess, onDeleteSuccess }: ServiceViewProps) {
    const [isEditing, setIsEditing] = useState(false);
    const [isConfirmingDelete, setIsConfirmingDelete] = useState(false);
    const [hasError, setHasError] = useState(false);

    const [serviceName, setServiceName] = useState(service.serviceName);
    const [price, setPrice] = useState<number | "">(Number(service.price) || 0);
    const [isActive, setIsActive] = useState(service.isActive);
    const [specializationId, setSpecializationId] = useState<number>(Number(service.specializationId));

    useEffect(() => {
        setServiceName(service.serviceName);
        setPrice(Number(service.price) || 0);
        setIsActive(service.isActive);
        setSpecializationId(Number(service.specializationId));
        setIsEditing(false);
        setHasError(false);
        setIsConfirmingDelete(false);
    }, [service]);

    const handleSave = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();

        const payload = {
            id: Number(service.id),
            serviceName: serviceName.trim(),
            price: Number(price),
            isActive,
            categoryId: Number(service.categoryId),
            specializationId: Number(specializationId),
        };

        const result = await servicesApi.updateService(Number(service.id), payload);

        if (result?.type === "ok") {
            setIsEditing(false);
            setHasError(false);
            onUpdateSuccess();
        } else {
            setHasError(true);
        }
    };

    const handleDelete = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();

        if (!isConfirmingDelete) {
            setIsConfirmingDelete(true);
            return;
        }

        const result = await servicesApi.deleteService(Number(service.id));

        if (result?.type === "ok") {
            setIsEditing(false);
            setHasError(false);
            setIsConfirmingDelete(false);
            onDeleteSuccess();
        } else {
            setHasError(true);
            setIsConfirmingDelete(false);
        }
    };

    if (isEditing) {
        return (
            <div className="service-card editing" onClick={(e) => e.stopPropagation()}>
                <input
                    type="text"
                    value={serviceName}
                    onChange={(e) => {
                        setServiceName(e.target.value);
                        if (hasError) setHasError(false);
                    }}
                />
                <input
                    type="number"
                    value={price}
                    onChange={(e) => {
                        const val = e.target.value;
                        setPrice(val === "" ? "" : Number(val));
                        if (hasError) setHasError(false);
                    }}
                />
                <select
                    value={specializationId}
                    onChange={(e) => {
                        setSpecializationId(Number(e.target.value));
                        if (hasError) setHasError(false);
                    }}
                >
                    {specializations.map((spec) => (
                        <option key={spec.id} value={spec.id}>
                            {spec.specializationName} {spec.isActive ? "" : "(Inactive)"}
                        </option>
                    ))}
                </select>
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={isActive}
                        onChange={(e) => setIsActive(e.target.checked)}
                    />
                    Active
                </label>
                <button
                    type="button"
                    className={hasError ? "btn-error" : ""}
                    disabled={hasError}
                    onClick={handleSave}
                >
                    Save
                </button>
                <button
                    type="button"
                    onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        setIsEditing(false);
                        setHasError(false);
                    }}
                >
                    Cancel
                </button>
            </div>
        );
    }
    
    const child = (<>
        <div className="service-info">
            <div className="service-name">{service.serviceName}</div>
            <span className={`service-status ${service.isActive ? "active" : "inactive"}`}>
                        {service.isActive ? "Active" : "Inactive"}
                    </span>
        </div>
        <div className="service-price">${service.price}</div>
    </>);

    return (
        <div className={`service-card ${service.isActive ? "active" : "inactive"}`}>
            <RequireRole roles={[Roles.Patient]}
                         fallback={
                             child
                         }>
                <Link to={`/make-appointment?serviceId=${service.id}&specId=${service.specializationId}`} className="service-link">
                    {child}
                </Link>
            </RequireRole>
            <RequireRole roles={[Roles.Receptionist]}>
                <div className="service-actions" onClick={(e) => e.stopPropagation()}>
                    <button type="button" onClick={() => setIsEditing(true)}>Edit</button>
                    <button type="button" className="btn-delete" onClick={handleDelete}>
                        {isConfirmingDelete ? "Confirm?" : "Delete"}
                    </button>
                    {isConfirmingDelete && (
                        <button type="button" onClick={() => setIsConfirmingDelete(false)}>Cancel</button>
                    )}
                </div>
            </RequireRole>
        </div>
    );
}

interface GroupedBySpecializationServicesProps {
    category: CategoryDto;
}

export function GroupedBySpecializationServices({ category }: GroupedBySpecializationServicesProps) {
    const [services, setServices] = useState<Array<ServiceDto>>();
    const [allSpecializations, setAllSpecializations] = useState<Array<SpecializationDto>>([]);
    const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>({});

    const [isManageModalOpen, setIsManageModalOpen] = useState(false);
    const [isCreatingGeneralService, setIsCreatingGeneralService] = useState(false);
    const [creatingServiceSpecId, setCreatingServiceSpecId] = useState<number | null>(null);

    const loadData = async () => {
        try {
            const [servicesRes, specsRes] = await Promise.all([
                servicesApi.getServices(Number(category.id)),
                servicesApi.getSpecializations(),
            ]);

            if (servicesRes.type === "ok") {
                setServices(servicesRes.value.services);
            } else {
                setServices([]);
            }

            if (specsRes.type === "ok") {
                setAllSpecializations(specsRes.value.specializations);
            }
        } catch (err) {
            console.log(err);
        }
    };

    useEffect(() => {
        loadData();
    }, [category.id]);

    if (!services) {
        return null;
    }

    const toggleGroup = (name: string) => {
        setExpandedGroups((prev) => ({ ...prev, [name]: !prev[name] }));
    };

    const groupedServices = groupBy(services, (i) => i.specializationName);

    return (
        <div className="groups-container">
            <RequireRole roles={[Roles.Receptionist]}>
                <div className="specialization-actions-header">
                    <button
                        type="button"
                        className="create-service-header-btn"
                        onClick={() => setIsCreatingGeneralService(true)}
                    >
                        + Create Service
                    </button>
                    <button
                        type="button"
                        className="create-spec-btn"
                        onClick={() => setIsManageModalOpen(true)}
                    >
                        ⚙ Manage Specializations
                    </button>
                </div>

                {isCreatingGeneralService && (
                    <div className="general-service-create-wrapper">
                        <ServiceCreateView
                            categoryId={Number(category.id)}
                            specializations={allSpecializations}
                            onSuccess={async () => {
                                setIsCreatingGeneralService(false);
                                await loadData();
                            }}
                            onCancel={() => setIsCreatingGeneralService(false)}
                        />
                    </div>
                )}
            </RequireRole>

            {Object.entries(groupedServices).map(([specializationName, items]) => {
                const isExpanded = expandedGroups[specializationName] || false;
                const specId = Number(items[0]?.specializationId);
                const specDto = allSpecializations.find((s) => Number(s.id) === specId);

                return (
                    <div key={specializationName} className="specialization-group">
                        <div
                            className={`specialization-header ${isExpanded ? "expanded" : ""}`}
                            onClick={() => toggleGroup(specializationName)}
                        >
                            <span className="arrow">{isExpanded ? "▼" : "▶"}</span>
                            <span className="specialization-title">{specializationName}</span>
                            {specDto && (
                                <span className={`service-status ${specDto.isActive ? "active" : "inactive"}`}>
                                    {specDto.isActive ? "Active" : "Inactive"}
                                </span>
                            )}
                            <span className="specialization-count">({items.length})</span>
                        </div>

                        {isExpanded && (
                            <div className="services-list">
                                {items.map((service) => (
                                    <ServiceView
                                        key={service.id}
                                        service={service}
                                        specializations={allSpecializations}
                                        onUpdateSuccess={loadData}
                                        onDeleteSuccess={loadData}
                                    />
                                ))}

                                <RequireRole roles={[Roles.Receptionist]}>
                                    {creatingServiceSpecId === specId ? (
                                        <ServiceCreateView
                                            categoryId={Number(category.id)}
                                            specializations={allSpecializations}
                                            defaultSpecializationId={specId}
                                            onSuccess={async () => {
                                                setCreatingServiceSpecId(null);
                                                await loadData();
                                            }}
                                            onCancel={() => setCreatingServiceSpecId(null)}
                                        />
                                    ) : (
                                        <button
                                            type="button"
                                            className="create-service-btn"
                                            onClick={() => setCreatingServiceSpecId(specId)}
                                        >
                                            + Add Service to {specializationName}
                                        </button>
                                    )}
                                </RequireRole>
                            </div>
                        )}
                    </div>
                );
            })}

            {isManageModalOpen && (
                <SpecializationsManagerModal
                    specializations={allSpecializations}
                    onClose={() => setIsManageModalOpen(false)}
                    onRefresh={loadData}
                />
            )}
        </div>
    );
}