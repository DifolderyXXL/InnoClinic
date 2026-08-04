import { useEffect, useState } from "react";
import { type CategoryDto, servicesApi } from "../../../../services/api/ServicesApi.ts";
import { DiscretePageSelector } from "../../Shared/PageSelector.tsx";
import { GroupedBySpecializationServices } from "./GroupBySpecializationServices.tsx";
import "./ServicesPage.css";

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

    useEffect(() => {
        const loadData = async () => {
            try {
                const result = await servicesApi.getCategories();
                if (result.type === "ok") {
                    const fetchedCategories = result.value.categories || result.value.items || [];
                    setCategories(fetchedCategories);
                    if (fetchedCategories.length > 0) {
                        setCategory(fetchedCategories[0]);
                    }
                } else {
                    setError(result.error?.title || "Error loading categories");
                }
            } catch {
                setError("Unhandled error");
            }
        };
        loadData();
    }, []);

    if (error) {
        return <div className="status-message error">{error}</div>;
    }

    if (!categories) {
        return <div className="status-message">Loading categories...</div>;
    }

    return (
        <div className="services-page-container">
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

            {category && (
                <div className="services-content">
                    <GroupedBySpecializationServices category={category} />
                </div>
            )}
        </div>
    );
}