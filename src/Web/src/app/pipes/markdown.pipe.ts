import { Pipe, PipeTransform } from '@angular/core';
import { marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import DOMPurify from 'dompurify';

@Pipe({
    name: 'markdown',
    standalone: true
})
export class MarkdownPipe implements PipeTransform {
    constructor(private sanitizer: DomSanitizer) { }

    transform(value: string | undefined): SafeHtml {
        if (!value) return '';

        // 1. Parse markdown to HTML
        const html = marked.parse(value) as string;

        // 2. Sanitize HTML to prevent XSS
        // Since we are getting content from external AI sources that process user-generated content,
        // we must never trust the HTML output directly.
        const cleanHtml = DOMPurify.sanitize(html);

        // 3. Tell Angular it is safe to render the cleaned HTML
        return this.sanitizer.bypassSecurityTrustHtml(cleanHtml);
    }
}
