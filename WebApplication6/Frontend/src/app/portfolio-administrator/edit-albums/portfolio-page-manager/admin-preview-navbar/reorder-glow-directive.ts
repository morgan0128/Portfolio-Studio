import {afterRenderEffect, Directive, ElementRef, inject, input, signal} from '@angular/core';

@Directive({
  selector: '[appReorderGlow]'
})
export class ReorderGlowDirective {
  readonly appReorderGlow = input(0);

  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef)
  private initialized = signal<boolean>(false);

  constructor() {
    afterRenderEffect(onCleanup => {
      const trigger = this.appReorderGlow();

      if (!this.initialized()){
        this.initialized.set(true);
        return;
      }

      if (trigger === 0) return;

      const htmlElement = this.element.nativeElement;
      const { borderColor, boxShadow } = getComputedStyle(htmlElement);

      const animation = htmlElement.animate(
        [
          { borderColor, boxShadow, offset: 0 },
          { borderColor: '#ccf3ff', boxShadow: '0 0 8px 2px #b1ddf3', offset: 0.05 },
          { borderColor, boxShadow, offset: 1 }
        ],
        {
          duration: 1000
        }
      );

      onCleanup(() => animation.cancel());
    });
  }

}
