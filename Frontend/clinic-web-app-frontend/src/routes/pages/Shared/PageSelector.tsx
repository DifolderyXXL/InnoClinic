import {useState} from "react";

interface PageSelectorProps {
    total: number;
    pageSize: number;
    onPageChange: (page: number) => void;
}

export function PageSelector({ total, pageSize, onPageChange }: PageSelectorProps) {
    const [currentPage, setCurrentPage] = useState(1);

    const totalPages = Math.ceil(total / pageSize);

    const goToPage = (page: number) => {
        if (page < 1 || page > totalPages) return;
        setCurrentPage(page);
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