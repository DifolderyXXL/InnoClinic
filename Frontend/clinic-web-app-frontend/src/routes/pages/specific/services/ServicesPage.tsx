import {useEffect, useState} from "react";
import {type CategoryDto, servicesApi, type SpecializationDto} from "../../../../services/api/ServicesApi.ts";
import {DiscretePageSelector} from "../../Shared/PageSelector.tsx";
import {groupBy} from "../../../../utilities/arrayUtils.ts";
import {GroupedBySpecializationServices} from "./GroupBySpecializationServices.tsx";

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

export function ServicesPage(){
    const [categories, setCategories] = useState<Array<CategoryDto>>();
    const [error, setError] = useState<string | null>(null);
    const [category, setCategory] = useState<CategoryDto | null>(null);

    useEffect(() => {
        const loadData = async () =>{
            try {
                const result = await servicesApi.getCategories();
                if (result.type === "ok") {
                    setCategories(result.value.categories);
                } else {
                    setError(result.error?.title || "Error");
                }
            } catch (err) {
                setError("Unhandled error");
            }
        }
        loadData();
    }, [category?.id]);
    
    if(error)
    {
        return (<div><p>{error}</p></div>);
    }
    
    if(!categories)
    {
        return <></>;
    }
    
    if(!category)
    {
        setCategory(categories[0])
    }
    
    return (
        <div>
            <DiscretePageSelector tabs={categories} onPageChange={setCategory} start={categories[0]} getId={x=>x.id}>
                {(activeTab: CategoryDto) => (
                    <>{activeTab.categoryName}</>
                )}
            </DiscretePageSelector>
            {category && <GroupedBySpecializationServices category={category}/>}
        </div>
    );
}
