import { ComponentFactoryResolver, Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AppIconFileComponent } from '../common/file-icons/file-icon.component';

@Directive({
    selector: '[icon]',
})
export class IConFileDirective {
    @Input() href: string;
    // constructor(
    //     private templateRef: TemplateRef<void>,
    //     private vcr: ViewContainerRef,
    //     private cfr: ComponentFactoryResolver) {

    // }

    constructor(public viewContainerRef: ViewContainerRef,
        private componentFactoryResolver: ComponentFactoryResolver) { }

    ngOnInit(): void {
        // this.vcr.createEmbeddedView(this.templateRef)
        // const cmpFactory = this.cfr.resolveComponentFactory(AppIconFileComponent)
        // this.vcr.createComponent(cmpFactory)

        const componentFactory = this.componentFactoryResolver.resolveComponentFactory(AppIconFileComponent);
        this.viewContainerRef.clear();

        const componentRef = this.viewContainerRef.createComponent(componentFactory);
        (<AppIconFileComponent>componentRef.instance).url = this.href;
    }
}