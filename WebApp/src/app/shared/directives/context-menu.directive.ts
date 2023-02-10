import { Directive, ViewContainerRef, Input, OnDestroy, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
import { Overlay, OverlayRef, ConnectionPositionPair, OverlayConfig } from '@angular/cdk/overlay';
import { Subscription, merge, fromEvent } from 'rxjs';
import { OVERLAY_POSITION_MAP } from '@constants';
import { TemplatePortal } from '@angular/cdk/portal';
import { IDropdownPanel } from '../common/dropdown/dropdown.component';
import { filter, take } from 'rxjs/operators';
import { ClassGetter } from '@angular/compiler/src/output/output_ast';

@Directive({
    selector: '[contextMenu]',
    host: {
        '(contextmenu)': 'open($event)'
    },
})
export class ContextMenuDirective implements OnDestroy {
    @Output() onTouch: EventEmitter<any> = new EventEmitter<any>();
    @Input('contextMenu') public menuTemplate: IDropdownPanel;
    @Input() set position(key: any) {
        if (!!key) {
            this._position = OVERLAY_POSITION_MAP[key];
        }
    }

    @Input() set activeMenuContext(isAllow: boolean) {
        this._isDisabled = isAllow;
    }

    get position() {
        return this._position;
    }

    get activeMenuContext() {
        return this._isDisabled;
    }

    private mouseX: number;
    private mouseY: number;

    private get overlayConfig(): OverlayConfig {
        return new OverlayConfig({
            //hasBackdrop: true,
            backdropClass: 'cdk-overlay-context-menu',
            scrollStrategy: this._overlay.scrollStrategies.close(),
            positionStrategy: this._overlay
                .position()
                .flexibleConnectedTo({ x: this.mouseX, y: this.mouseY })
                .withPositions([
                    {
                        originX: 'end',
                        originY: 'top',
                        overlayX: 'start',
                        overlayY: 'top',
                    },
                    {
                        originX: 'start',
                        originY: 'top',
                        overlayX: 'end',
                        overlayY: 'top',
                    },
                    {
                        originX: 'end',
                        originY: 'bottom',
                        overlayX: 'start',
                        overlayY: 'bottom',
                    },
                    {
                        originX: 'start',
                        originY: 'bottom',
                        overlayX: 'end',
                        overlayY: 'bottom',
                    },
                ])
        });
    }

    private overlayRef: OverlayRef;
    private dropdownClosingActions$: Subscription = Subscription.EMPTY;
    private _position: ConnectionPositionPair = OVERLAY_POSITION_MAP.rightTop;
    private _isDisabled: boolean = false;

    constructor(
        private readonly _overlay: Overlay,
        private readonly viewContainerRef: ViewContainerRef) {
    }

    public open(e: MouseEvent) {
        if (!!this._isDisabled) {
            return;
        }
        e.preventDefault();
        this.onTouch.emit();
        this.openContextMenu(e);
    }

    private openContextMenu({ x, y }: MouseEvent) {
        this.close();
        this.mouseX = x;
        this.mouseY = y;
        this.overlayRef = this._overlay.create(this.overlayConfig);
        const windowHeight = window.screen.availHeight;
        let contextMenuHeight = 0;
        if (this.menuTemplate && this.menuTemplate.templateRef.elementRef.nativeElement.nextElementSibling) {
          contextMenuHeight = this.menuTemplate.templateRef.elementRef.nativeElement.nextElementSibling.offsetHeight;
        }
        if (this.mouseY + 200 > windowHeight) {
            this.overlayConfig.positionStrategy = this._overlay.position().flexibleConnectedTo({ x: this.mouseX, y: this.mouseY }).withPositions([
                {
                    originX: 'end',
                    originY: 'top',
                    overlayX: 'start',
                    overlayY: 'top',
                  },
                  {
                    originX: 'start',
                    originY: 'top',
                    overlayX: 'end',
                    overlayY: 'top',
                  },
                  {
                    originX: 'end',
                    originY: 'bottom',
                    overlayX: 'start',
                    overlayY: 'bottom',
                  },
                  {
                    originX: 'start',
                    originY: 'bottom',
                    overlayX: 'end',
                    overlayY: 'bottom',
                  },
            ]);
        }
        this.overlayRef.attach(new TemplatePortal(
          this.menuTemplate.templateRef, this.viewContainerRef
        ));
        console.log('a', this.menuTemplate.templateRef);
        // Listen Event Closing
        this.dropdownClosingActions$ = this.onClosingDropdown()
          .subscribe(
            () => this.close()
          );
      }

    private onClosingDropdown() {
        // const backdropClick$ = this.overlayRef.backdropClick(); // ? clickoutside 
        const detachment$ = this.overlayRef.detachments();
        const dropdownClosed$ = this.menuTemplate.closed;   // ? dropdown emit close
        // const dropdownClickOutSide$ = this.menuTemplate.clickOutside;   // ? dropdown emit close

        const dropdownClickOutSide$ = fromEvent<MouseEvent>(document, 'click')
            .pipe(
                filter(event => {
                    const clickTarget = event.target as HTMLElement;
                    return !!this.overlayRef && !this.overlayRef.overlayElement.contains(clickTarget);
                }),
                take(1)
            );

        return merge(detachment$, dropdownClosed$, dropdownClickOutSide$);
    }

    public close() {
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