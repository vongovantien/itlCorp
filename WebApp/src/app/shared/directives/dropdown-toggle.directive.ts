import { Directive, OnDestroy, ElementRef, ViewContainerRef, Input } from '@angular/core';
import { Overlay, OverlayRef, OverlayConfig } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';
import { Subscription, merge } from 'rxjs';

@Directive({
    selector: '[dropdownToggle]',
    host: {
        '(click)': 'toggleDropdown()'
    }
})
export class DropdownToggleDirective implements OnDestroy {

    @Input('dropdownToggle') public dropdownPanel: any;

    private isOpenDropdown: boolean = false;
    private overlayRef: OverlayRef;

    private readonly overlayConfig: OverlayConfig = {
        hasBackdrop: true,
        backdropClass: 'cdk-overlay-dropdown-backdrop',
        scrollStrategy: this._overlay.scrollStrategies.close(),
        positionStrategy: this._overlay
            .position()
            .flexibleConnectedTo(this._elementRef)
            .withPositions([
                {
                    originX: 'end',
                    originY: 'bottom',
                    overlayX: 'end',
                    overlayY: 'top',
                    offsetY: 8
                }
            ])
    };

    private dropdownClosingActions$: Subscription = Subscription.EMPTY;

    private readonly templatePortal: TemplatePortal<any> = new TemplatePortal(
        this.dropdownPanel.templateRef,
        this._viewContainerRef
    );

    constructor(
        private readonly _overlay: Overlay,
        private readonly _elementRef: ElementRef<HTMLElement>,
        private readonly _viewContainerRef: ViewContainerRef
    ) { }


    toggleDropdown() {
        this.isOpenDropdown ? this.destroyDropdown() : this.openDropdown();
    }

    openDropdown() {
        this.isOpenDropdown = true;

        this.overlayRef = this._overlay.create(this.overlayConfig);

        this.overlayRef.attach(this.templatePortal);


        // Listen Event Closing
        this.dropdownClosingActions$ = this.onClosingDropdown()
            .subscribe(
                () => this.destroyDropdown()
            );
    }

    destroyDropdown() {
        if (!this.overlayRef || !this.isOpenDropdown) {
            return;
        }

        this.dropdownClosingActions$.unsubscribe();
        this.isOpenDropdown = false;
        this.overlayRef.detach();
    }

    private onClosingDropdown() {
        const backdropClick$ = this.overlayRef.backdropClick();
        const detachment$ = this.overlayRef.detachments();
        const dropdownClosed = this.dropdownPanel.closed;

        return merge(backdropClick$, detachment$, dropdownClosed);
    }


    ngOnDestroy(): void {
        if (this.overlayRef) {
            this.overlayRef.dispose();
        }
    }


}