import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
    selector: 'textarea[autoExpand]'
})
export class AutoExpandDirective {
    private _minRows!: number;

    constructor(private element: ElementRef) { }

    @HostListener('input', ['$event.target'])
    onInput(textArea: HTMLTextAreaElement): void {
        this.adjustTextArea(textArea);
    }

    private adjustTextArea(textArea: HTMLTextAreaElement): void {
        textArea.style.overflow = 'hidden';
        textArea.style.height = 'auto';
        textArea.style.height = `${textArea.scrollHeight}px`;
    }

    // ngAfterViewInit(): void {
    //     setTimeout(() => {
    //         this.adjustTextArea(this.element.nativeElement);
    //     });
    // }

    ngAfterContentChecked() {
        this.adjustTextArea(this.element.nativeElement);
    }
}