import React, {useState} from "react";
import "./PageSelector.css"

interface PageSelectorProps {
    currentPage: number;
    total: number;
    pageSize?: number;
    onPageChange: (page: number) => void;
}

export function PageSelector({ currentPage, total, pageSize = 50, onPageChange }: PageSelectorProps) {
    const totalPages = Math.ceil(total / pageSize);

    const goToPage = (page: number) => {
        if (page < 1 || page > totalPages) return;
        onPageChange(page);
    };

    return (
        <div>
            <button onClick={() => goToPage(currentPage - 1)} disabled={currentPage === 1}>
                Prev
            </button>
            <span>Page {currentPage} of {totalPages}</span>
            <button onClick={() => goToPage(currentPage + 1)} disabled={currentPage === totalPages}>
                Next
            </button>
        </div>
    );
}

interface DiscretePageSelectorProps<T>{
    tabs: Array<T>;
    start?: T;
    
    onPageChange: (tab: T) => void;
    getId: (tab: T) => string | number;

    children: (activeTab: T) => React.ReactNode;
}

export function DiscretePageSelector<T>({tabs, onPageChange, children, start, getId}: DiscretePageSelectorProps<T>) {
    if(!tabs) return (<></> );
    
    const [currentPage, setCurrentPage] = useState<string|number|null>(start ? getId(start) : null);

    const goToPage = (tab: any) => {
        setCurrentPage(getId(tab));
        onPageChange(tab);
    };

    const listItems = tabs.map(tab => {
        const id = getId(tab);
        const isActive = currentPage === id;
        return (
            <button
                key={id}
                className={`discreteTabButton ${isActive ? 'active' : ''}`}
                onClick={() => goToPage(tab)}
                disabled={isActive}
            >
                {children(tab)}
            </button>
        );
    });
    
    return (
        <div>
            {listItems}
        </div>
    );
}