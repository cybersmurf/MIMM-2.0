/**
 * MoodSelector2D - Pure JavaScript implementation with direct DOM event listeners
 */
class MoodSelectorJS {
    constructor(canvasId, size, dotNetRef) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error('Canvas not found:', canvasId);
            return;
        }
        
        this.size = size;
        this.dotNetRef = dotNetRef;
        this.isDragging = false;
        this.valence = 0;
        this.arousal = 0;
        
        this.setupEventListeners();
        this.draw();
    }
    
    setupEventListeners() {
        // Mouse events
        this.canvas.addEventListener('mousedown', (e) => {
            this.isDragging = true;
            this.handlePointerEvent(e);
        });
        
        this.canvas.addEventListener('mousemove', (e) => {
            if (this.isDragging) {
                this.handlePointerEvent(e);
            }
        });
        
        this.canvas.addEventListener('mouseup', () => {
            this.isDragging = false;
        });
        
        this.canvas.addEventListener('mouseleave', () => {
            this.isDragging = false;
        });
        
        // Touch events
        this.canvas.addEventListener('touchstart', (e) => {
            e.preventDefault();
            this.isDragging = true;
            const touch = e.touches[0];
            this.handleTouchEvent(touch);
        });
        
        this.canvas.addEventListener('touchmove', (e) => {
            e.preventDefault();
            if (this.isDragging && e.touches.length > 0) {
                const touch = e.touches[0];
                this.handleTouchEvent(touch);
            }
        });
        
        this.canvas.addEventListener('touchend', () => {
            this.isDragging = false;
        });
    }
    
    handlePointerEvent(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        this.updatePosition(x, y);
    }
    
    handleTouchEvent(touch) {
        const rect = this.canvas.getBoundingClientRect();
        const x = touch.clientX - rect.left;
        const y = touch.clientY - rect.top;
        this.updatePosition(x, y);
    }
    
    updatePosition(x, y) {
        // Clamp coordinates
        x = Math.max(0, Math.min(x, this.size));
        y = Math.max(0, Math.min(y, this.size));
        
        // Convert to valence/arousal (-1 to 1)
        this.valence = (x / this.size) * 2 - 1;
        this.arousal = 1 - (y / this.size) * 2;
        
        // Notify Blazor
        this.dotNetRef.invokeMethodAsync('OnPositionChanged', this.valence, this.arousal);
        
        // Redraw
        this.draw();
    }
    
    draw() {
        const ctx = this.canvas.getContext('2d');
        
        // Clear
        ctx.clearRect(0, 0, this.size, this.size);
        
        // Background
        ctx.fillStyle = 'rgba(230, 245, 250, 1)';
        ctx.fillRect(0, 0, this.size, this.size);
        
        // Grid lines
        ctx.strokeStyle = 'rgba(0, 0, 0, 0.1)';
        ctx.lineWidth = 1;
        for (let i = 1; i <= 3; i++) {
            const pos = (this.size / 4) * i;
            // Vertical
            ctx.beginPath();
            ctx.moveTo(pos, 0);
            ctx.lineTo(pos, this.size);
            ctx.stroke();
            // Horizontal
            ctx.beginPath();
            ctx.moveTo(0, pos);
            ctx.lineTo(this.size, pos);
            ctx.stroke();
        }
        
        // Cursor
        const cursorX = ((this.valence + 1) / 2) * this.size;
        const cursorY = ((1 - this.arousal) / 2) * this.size;
        
        ctx.fillStyle = 'white';
        ctx.strokeStyle = 'rgb(59, 130, 246)';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.arc(cursorX, cursorY, 10, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();
        
        // Axis labels
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.font = '11px Arial';
        ctx.textAlign = 'left';
        ctx.fillText('Negative', 8, this.size / 2 + 4);
        ctx.textAlign = 'right';
        ctx.fillText('Positive', this.size - 8, this.size / 2 + 4);
        ctx.textAlign = 'center';
        ctx.fillText('High arousal', this.size / 2, 16);
        ctx.fillText('Low arousal', this.size / 2, this.size - 8);
    }
    
    setValues(valence, arousal) {
        this.valence = valence;
        this.arousal = arousal;
        this.draw();
    }
    
    destroy() {
        // Clean up event listeners
        this.canvas.replaceWith(this.canvas.cloneNode(true));
    }
}

// Global store for instances
window.moodSelectorInstances = window.moodSelectorInstances || {};

window.initMoodSelector = function(canvasId, size, dotNetRef) {
    // Destroy existing instance if any
    if (window.moodSelectorInstances[canvasId]) {
        window.moodSelectorInstances[canvasId].destroy();
    }
    
    // Create new instance
    const instance = new MoodSelectorJS(canvasId, size, dotNetRef);
    window.moodSelectorInstances[canvasId] = instance;
    return true;
};

window.updateMoodSelectorValues = function(canvasId, valence, arousal) {
    const instance = window.moodSelectorInstances[canvasId];
    if (instance) {
        instance.setValues(valence, arousal);
    }
};

window.destroyMoodSelector = function(canvasId) {
    const instance = window.moodSelectorInstances[canvasId];
    if (instance) {
        instance.destroy();
        delete window.moodSelectorInstances[canvasId];
    }
};

// Old compatibility functions (can be removed later)
window.drawMoodCanvas = function() { /* deprecated */ };
window.getCanvasRelativePosition = function() { return [0, 0]; };
window.getRelativePositionById = function() { return [0, 0, 0, 0]; };
