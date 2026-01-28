/**
 * Gets the position relative to an element by ID, accounting for nested children
 * @param {string} elementId - The ID of the container element
 * @param {number} clientX - Mouse/touch clientX coordinate
 * @param {number} clientY - Mouse/touch clientY coordinate
 * @returns {number[]} Array with [x, y] relative to element
 */
window.getRelativePositionById = function(elementId, clientX, clientY) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error('Element not found:', elementId);
        return [0, 0, 0, 0];
    }
    
    const rect = element.getBoundingClientRect();
    const x = clientX - rect.left;
    const y = clientY - rect.top;
    return [x, y, rect.width, rect.height];
};
