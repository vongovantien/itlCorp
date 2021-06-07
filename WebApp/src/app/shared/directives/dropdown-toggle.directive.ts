import { Directive, OnDestroy, ElementRef, ViewContainerRef, Input } from '@angular/core';
import { Overlay, OverlayRef, OverlayConfig } from '@angular/cdk/overlay';
import { OVERLAY_POSITION_MAP } from '@constants';
import { TemplatePortal } from '@angular/cdk/portal';

import { Subscription, merge } from 'rxjs';
import { IDropdownPanel } from '@common';
@Directive({
    selector: '[dpToggle]',
    host: {
        '(click)': 'toggleDropdown()'
    },
})
export class DropdownToggleDirective implements OnDestroy {

    @Input('dpToggle') public dropdownPanel: IDropdownPanel;

    private isOpenDropdown: boolean = false;
    private overlayRef: OverlayRef;
    private dropdownClosingActions$: Subscription = Subscription.EMPTY;

    private get overlayConfig(): OverlayConfig {
        return new OverlayConfig({
            hasBackdrop: true,
            backdropClass: 'cdk-overlay-dropdown-backdrop',
            scrollStrategy: this._overlay.scrollStrategies.close(),
            positionStrategy: this._overlay
                .position()
                .flexibleConnectedTo(this._elementRef)
                .withPositions([
                    OVERLAY_POSITION_MAP.topRight
                ])
        });
    }
    constructor(
        private readonly _overlay: Overlay,
        private readonly _elementRef: ElementRef<HTMLElement>,
        private readonly _viewContainerRef: ViewContainerRef,
    ) { }


    public toggleDropdown() {
        this.isOpenDropdown ? this.destroyDropdown() : this.openDropdown();
    }

    private openDropdown() {
        this.isOpenDropdown = true;

        this.overlayRef = this._overlay.create(this.overlayConfig);

        this.overlayRef.attach(new TemplatePortal(
            this.dropdownPanel.templateRef,
            this._viewContainerRef
        ));

        // Listen Event Closing
        this.onClosingDropdown()
            .subscribe(
                () => this.destroyDropdown()
            );
    }

    private destroyDropdown() {
        if (!this.overlayRef || !this.isOpenDropdown) {
            return;
        }

        this.dropdownClosingActions$.unsubscribe();
        this.isOpenDropdown = false;
        this.overlayRef.detach();
    }

    private onClosingDropdown() {
        const backdropClick$ = this.overlayRef.backdropClick(); // ? clickoutside 
        const detachment$ = this.overlayRef.detachments();
        const dropdownClosed$ = this.dropdownPanel.closed;   // ? dropdown emit close

        return merge(backdropClick$, detachment$, dropdownClosed$);
    }

    ngOnDestroy(): void {
        if (this.overlayRef) {
            this.overlayRef.dispose();
        }
    }
}