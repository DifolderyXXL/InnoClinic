import { useEffect, useState } from "react";
import { type CategoryDto, servicesApi } from "../../../../services/api/ServicesApi.ts";
import { DiscretePageSelector } from "../../Shared/PageSelector.tsx";
import { GroupedBySpecializationServices } from "./GroupBySpecializationServices.tsx";
import "./ServicesPage.css";
import { RequireRole, Roles } from "../../../../components/common/RequireRole.tsx";

export interface ServiceDto {
    id: number;
    serviceName: string;
    price: number;
    isActive: boolean;

    categoryId: number;
    categoryName: string;

    specializationId: number;
    specializationName: string;
}

export function ServicesPage() {
    const [categories, setCategories] = useState<Array<CategoryDto> | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [category, setCategory] = useState<CategoryDto | null>(null);
    const [isCreating, setIsCreating] = useState(false);

    const loadData = async (targetCategoryId?: number) => {
        try {
            const result = await servicesApi.getCategories();
            if (result.type === "ok") {
                const fetchedCategories = result.value.categories || result.value.items || [];
                setCategories(fetchedCategories);

                if (fetchedCategories.length > 0) {
                    const activeId = targetCategoryId ?? category?.id;
                    const found = fetchedCategories.find((c: CategoryDto) => Number(c.id) === Number(activeId));
                    setCategory(found || fetchedCategories[0]);
                } else {
                    setCategory(null);
                }
            } else {
                setError(result.error?.title || "Error loading categories");
            }
        } catch {
            setError("Unhandled error");
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const handleCategoryCreated = async (newCategory?: CategoryDto) => {
        setIsCreating(false);
        await loadData(newCategory?.id ? Number(newCategory.id) : undefined);
    };

    const handleCategoryUpdated = async (updatedCategory: CategoryDto) => {
        await loadData(Number(updatedCategory.id));
    };

    const handleCategoryDeleted = async () => {
        await loadData();
    };

    if (error) {
        return <div className="status-message error">{error}</div>;
    }

    if (!categories) {
        return <div className="status-message">Loading categories...</div>;
    }

    return (
        <div className="services-page">
            <div className="services-page-container">
                {isCreating ? (
                    <CategoryCreateView
                        onSuccess={handleCategoryCreated}
                        onCancel={() => setIsCreating(false)}
                    />
                ) : (
                    category && (
                        <CategoryView
                            category={category}
                            onUpdateSuccess={handleCategoryUpdated}
                            onDeleteSuccess={handleCategoryDeleted}
                        />
                    )
                )}
                <div className="categories-header">
                    <DiscretePageSelector
                        tabs={categories}
                        onPageChange={setCategory}
                        start={category || categories[0]}
                        getId={x => x.id}
                    >
                        {(activeTab: CategoryDto) => (
                            <span>{activeTab.categoryName}</span>
                        )}
                    </DiscretePageSelector>

                    <RequireRole roles={[Roles.Receptionist]}>
                        <button
                            type="button"
                            className="create-category-btn discreteTabButton"
                            onClick={() => setIsCreating(true)}
                        >
                            + Create
                        </button>
                    </RequireRole>
                </div>

                {category && (
                    <div className="services-content">
                        <GroupedBySpecializationServices category={category} />
                    </div>
                )}
            </div>
        </div>
    );
}

interface CategoryCreateViewProps {
    onSuccess: (category?: CategoryDto) => void;
    onCancel: () => void;
}

export function CategoryCreateView({ onSuccess, onCancel }: CategoryCreateViewProps) {
    const [categoryName, setCategoryName] = useState("");
    const [timeSlotSize, setTimeSlotSize] = useState<number | "">("");
    const [hasError, setHasError] = useState(false);

    const handleCreate = async () => {
        if (!categoryName.trim() || timeSlotSize === "") {
            setHasError(true);
            return;
        }

        const payload = { categoryName: categoryName.trim(), timeSlotSize: Number(timeSlotSize) };
        const result = await servicesApi.createCategory(payload);

        if (result?.type === "ok") {
            onSuccess(result.value);
        } else {
            setHasError(true);
        }
    };

    return (
        <div className="category-view-container">
            <input
                type="text"
                placeholder="Category name"
                value={categoryName}
                onChange={(e) => {
                    setCategoryName(e.target.value);
                    if (hasError) setHasError(false);
                }}
            />
            <input
                type="number"
                placeholder="Slot size"
                value={timeSlotSize}
                onChange={(e) => {
                    const val = e.target.value;
                    setTimeSlotSize(val === "" ? "" : Number(val));
                    if (hasError) setHasError(false);
                }}
            />
            <div className="category-view-actions">
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
        </div>
    );
}

interface CategoryViewProps {
    category: CategoryDto;
    onUpdateSuccess?: (updated: CategoryDto) => void;
    onDeleteSuccess?: (deletedId: number) => void;
}

export function CategoryView({ category, onUpdateSuccess, onDeleteSuccess }: CategoryViewProps) {
    const [isEditing, setIsEditing] = useState(false);
    const [hasError, setHasError] = useState(false);
    const [isConfirmingDelete, setIsConfirmingDelete] = useState(false);
    const [categoryName, setCategoryName] = useState(category.categoryName);
    const [timeSlotSize, setTimeSlotSize] = useState<number | "">(category.timeSlotSize);

    useEffect(() => {
        setCategoryName(category.categoryName);
        setTimeSlotSize(category.timeSlotSize);
        setIsEditing(false);
        setHasError(false);
        setIsConfirmingDelete(false);
    }, [category]);

    const handleDelete = async () => {
        if (!isConfirmingDelete) {
            setIsConfirmingDelete(true);
            return;
        }

        const result = await servicesApi.deleteCategory(Number(category.id));

        if (result?.type === "ok") {
            setIsEditing(false);
            setHasError(false);
            setIsConfirmingDelete(false);
            if (onDeleteSuccess) onDeleteSuccess(Number(category.id));
        } else {
            setHasError(true);
            setIsConfirmingDelete(false);
        }
    };

    const handleSave = async () => {
        const payload = { categoryName, timeSlotSize: Number(timeSlotSize) };
        const result = await servicesApi.updateCategory(Number(category.id), payload);

        if (result?.type === "ok") {
            setIsEditing(false);
            setHasError(false);
            if (onUpdateSuccess) onUpdateSuccess({ ...category, ...payload });
        } else {
            setHasError(true);
        }
    };

    if (isEditing) {
        return (
            <div className="category-view-container">
                <input
                    type="text"
                    value={categoryName}
                    onChange={(e) => {
                        setCategoryName(e.target.value);
                        if (hasError) setHasError(false);
                    }}
                />
                <input
                    type="number"
                    value={timeSlotSize}
                    onChange={(e) => {
                        const val = e.target.value;
                        setTimeSlotSize(val === "" ? "" : Number(val));
                        if (hasError) setHasError(false);
                    }}
                />
                <div className="category-view-actions">
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
                        onClick={() => {
                            setIsEditing(false);
                            setHasError(false);
                        }}
                    >
                        Cancel
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="category-view-container">
            <span>{category.categoryName}</span>
            <span>{category.timeSlotSize} time slots</span>
            <RequireRole roles={[Roles.Receptionist]}>
                <div className="category-view-actions">
                    <button type="button" onClick={() => setIsEditing(true)}>Edit</button>
                    <button
                        type="button"
                        className="btn-delete"
                        onClick={handleDelete}
                    >
                        {isConfirmingDelete ? "Confirm?" : "Delete"}
                    </button>
                    {isConfirmingDelete && (
                        <button
                            type="button"
                            onClick={() => setIsConfirmingDelete(false)}
                        >
                            Cancel
                        </button>
                    )}
                </div>
            </RequireRole>
        </div>
    );
}