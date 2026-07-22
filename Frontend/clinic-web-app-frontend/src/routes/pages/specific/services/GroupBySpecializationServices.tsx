import {groupBy} from "../../../../utilities/arrayUtils.ts";
import {type CategoryDto, servicesApi} from "../../../../services/api/ServicesApi.ts";
import {useEffect, useState} from "react";
import type {ServiceDto} from "./ServicesPage.tsx";
import "./ServicesPage.css"
import {Link} from "react-router-dom";

interface ServiceViewProps{
    service: ServiceDto;
}
export function ServiceView({service}:ServiceViewProps){
    return (
        <Link to={`view-services?id=${service.id}`} className="service-link">
            <div className={`service-card ${service.isActive ? "active" : ""}`}>
                <div>Name {service.serviceName}</div>
                <div>Price {service.price}</div>
            </div>
        </Link>
    );
}

interface GroupedBySpecializationServicesProps{
    category: CategoryDto
}
export function GroupedBySpecializationServices({category}: GroupedBySpecializationServicesProps){
    const [services, setServices] = useState<Array<ServiceDto>>();
    const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>({});

    useEffect(() => {
        const loadData = async () =>{
            try {
                const result = await servicesApi.getServices(category.id);
                if (result.type === "ok") {
                    setServices(result.value.services);
                }
                else{
                    setServices([]);
                }
            } catch (err) {
                console.log(err)
            }
        }
        loadData();
    }, [category.id]);

    if(!services)
    {
        return <></>
    }

    const toggleGroup = (name: string) => {
        setExpandedGroups(prev => ({ ...prev, [name]: !prev[name] }));
    };

    const groupedServices = groupBy(services, i => i.specializationName);

    const groups = Object.entries(groupedServices).map(([specializationName, items]) => {
        const isExpanded = expandedGroups[specializationName] || false;

        return (
            <div key={specializationName} className="specialization-group">
                <div onClick={() => toggleGroup(specializationName)} style={{ cursor: 'pointer' }}>
                    <span>{isExpanded ? '▼' : '▶'}</span> {specializationName} ({items.length})
                </div>
                {isExpanded && (
                    <div className="services-list">
                        {items.map((service) => (
                            <div key={service.id}>
                                <ServiceView service={service}/>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        );
    });

    return (
        <div className="groups-container">
            {groups}
        </div>
    );
}