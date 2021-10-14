import { Directive, ViewContainerRef, ElementRef, Input, OnDestroy } from '@angular/core';
import { Overlay, OverlayRef, ConnectionPositionPair, OverlayConfig } from '@angular/cdk/overlay';
import { Subscription, merge } from 'rxjs';
import { OVERLAY_POSITION_MAP } from '@constants';
import { TemplatePortal } from '@angular/cdk/portal';
import { IDropdownPanel } from '../common/dropdown/dropdown.component';

@Directive({
    selector: '[contextMenu]',
    host: {
        '(contextmenu)': 'open($event)'
    },
})
export class ContextMenuDirective implements OnDestroy {

    @Input('contextMenu') public menuTemplate: IDropdownPanel;
    @Input() set position(key: any) {
        if (!!key) {
            this._position = OVERLAY_POSITION_MAP[key];
        }
    }

    get position() {
        return this._position;
    }

    private get overlayConfig(): OverlayConfig {
        return new OverlayConfig({
            hasBackdrop: true,
            backdropClass: 'cdk-overlay-context-menu',
            scrollStrategy: this._overlay.scrollStrategies.close(),
            positionStrategy: this._overlay
                .position()
                .flexibleConnectedTo(this._elementRef)
                .withPositions([
                    this.position
                ])
        });
    }

    private overlayRef: OverlayRef;
    private dropdownClosingActions$: Subscription = Subscription.EMPTY;
    private _position: ConnectionPositionPair = OVERLAY_POSITION_MAP.rightTop;
    constructor(
        private readonly _overlay: Overlay,
        private readonly _elementRef: ElementRef<HTMLElement>,
        private readonly viewContainerRef: ViewContainerRef) {
    }

    public open(e: MouseEvent) {
        e.preventDefault();
        this.openContextMenu();

    }

    private openContextMenu() {
        this.close();
        this.overlayRef = this._overlay.create(this.overlayConfig);

        this.overlayRef.attach(new TemplatePortal(
            this.menuTemplate.templateRef, this.viewContainerRef
        ));

        // Listen Event Closing
        this.dropdownClosingActions$ = this.onClosingDropdown()
            .subscribe(
                () => this.close()
            );
    }

    private onClosingDropdown() {
        const backdropClick$ = this.overlayRef.backdropClick(); // ? clickoutside 
        const detachment$ = this.overlayRef.detachments();
        const dropdownClosed$ = this.menuTemplate.closed;   // ? dropdown emit close

        return merge(backdropClick$, detachment$, dropdownClosed$);
    }

    private close() {
        this.dropdownClosingActions$ && this.dropdownClosingActions$.unsubscribe();
        if (this.overlayRef) {
            this.overlayRef.dispose();
            this.overlayRef = null;
        }
    }


    ngOnDestroy(): void {
        this.close();
    }
}